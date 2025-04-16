using Microsoft.EntityFrameworkCore;
using YuGiTournament.Api.Abstractions;
using YuGiTournament.Api.ApiResponses;
using YuGiTournament.Api.Models;
using YuGiTournament.Api.Services.Abstractions;

namespace YuGiTournament.Api.Services
{
    public class PlayerService : IPlayerService
    {

        private readonly IUnitOfWork _unitOfWork;

        public PlayerService(IUnitOfWork unitOfWork)
        {

            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Player>> GetAllPlayersAsync()
        {
            var league = await _unitOfWork.GetRepository<LeagueId>()
                 .Find(x => x.IsFinished == false)
                 .FirstOrDefaultAsync();

            return league == null
                ? []
                : (IEnumerable<Player>)await _unitOfWork.GetRepository<Player>().GetAll().Where(x => x.LeagueNumber == league.Id).ToListAsync();
        }

        public async Task<Player?> GetPlayerByIdAsync(int playerId)
        {
            return await _unitOfWork.GetRepository<Player>().Find(p => p.PlayerId == playerId).FirstOrDefaultAsync();
        }

        public async Task<ApiResponse> DeletePlayerAsync(int playerId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var dbContext = _unitOfWork.GetDbContext();

                var playerToDelete = await _unitOfWork.GetRepository<Player>().Find(p => p.PlayerId == playerId).FirstOrDefaultAsync();
                if (playerToDelete == null)
                    return new ApiResponse(false, "مفيش هنا لاعب بالاسم ده");

                var playerMatches = await _unitOfWork.GetRepository<Match>()
                    .GetAll()
                    .Include(m => m.Rounds)
                    .Include(m => m.Player1)
                    .Include(m => m.Player2)
                    .Where(m => m.Player1Id == playerId || m.Player2Id == playerId)
                    .ToListAsync();

                foreach (var match in playerMatches)
                {
                    var opponent = match.Player1Id == playerId ? match.Player2 : match.Player1;
                    if (opponent == null)
                        continue;

                    foreach (var round in match.Rounds)
                    {
                        switch (round.WinnerId)
                        {
                            case var winnerId when winnerId == playerId:
                                opponent.Losses--;
                                break;
                            case var winnerId when winnerId == opponent.PlayerId:
                                opponent.Wins--;
                                opponent.Points--;
                                break;
                            case null when round.IsDraw:
                                opponent.Draws--;
                                opponent.Points -= 0.5;
                                break;
                        }
                    }

                    opponent.UpdateStats();
                }

                var matchRounds = playerMatches.SelectMany(m => m.Rounds).ToList();
                if (matchRounds.Any())
                    _unitOfWork.GetRepository<MatchRound>().DeleteRange(matchRounds);

                if (playerMatches.Any())
                    _unitOfWork.GetRepository<Match>().DeleteRange(playerMatches);

                _unitOfWork.GetRepository<Player>().Delete(playerToDelete);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                await dbContext.Database.ExecuteSqlRawAsync("EXEC UpdatePlayersRanking");

                return new ApiResponse(true, $"تم حذف اللاعب {playerToDelete.FullName} وكل مبارياته والجولات المرتبطة بيه، وتم تعديل إحصائيات اللاعبين الآخرين.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return new ApiResponse(false, $"حصل خطأ أثناء حذف اللاعب: {ex.Message}");
            }
        }

        public async Task<ApiResponse> AddPlayerAsync(string fullName)
        {
            var player = new Player { FullName = fullName };
            var dbContext = _unitOfWork.GetDbContext();


            var league = await _unitOfWork.GetRepository<LeagueId>()
                .Find(x => x.IsFinished == false)
                .FirstOrDefaultAsync();

            if (league == null)
                return new ApiResponse(false, $"لا يوجد دوري حاليا  ");

            var ExistPlayer = await _unitOfWork.GetRepository<Player>().Find(x => x.FullName == fullName).Where(p => p.LeagueNumber == league.Id).FirstOrDefaultAsync();

            if (ExistPlayer != null)
                return new ApiResponse(false, $"تم اضافة اللاعب {player.FullName} من قبل !!!");

            await _unitOfWork.GetRepository<Player>().AddAsync(player);
            player.LeagueNumber = league.Id;

            await _unitOfWork.SaveChangesAsync();

            var otherPlayers = await _unitOfWork.GetRepository<Player>()
                .GetAll()
                .Where(p => p.PlayerId != player.PlayerId && p.LeagueNumber == league.Id)
                .ToListAsync();

            var matches = new List<Match>();





            foreach (var opponent in otherPlayers)
            {
                matches.Add(new Match
                {
                    Player1Id = player.PlayerId,
                    Player2Id = opponent.PlayerId,
                    Score1 = 0,
                    Score2 = 0,
                    IsCompleted = false,
                    LeagueNumber = league.Id
                });
            }

            if (matches.Count != 0)
            {
                await _unitOfWork.GetRepository<Match>().AddRangeAsync(matches);
                await _unitOfWork.SaveChangesAsync();
                await dbContext.Database.ExecuteSqlRawAsync("EXEC UpdatePlayersRanking");

            }

            return new ApiResponse(true, $"تم اضافة اللاعب {player.FullName} وكل مبارياته والجولات المرتبطة بيه.");
        }

        public async Task<IEnumerable<Player>> GetPlayersRankingAsync()
        {
            var league = await _unitOfWork.GetRepository<LeagueId>()
                .Find(x => x.IsFinished == false)
                .FirstOrDefaultAsync();

            if (league == null)
                return [];

            var players = await _unitOfWork.GetRepository<Player>()
                .GetAll()
                .Where(x => x.LeagueNumber == league.Id)
                .OrderBy(x => x.Rank)
                .ToListAsync();

            return players;
        }

        public async Task<IEnumerable<object>> GetAllLeaguesWithRankAsync()
        {
            var leagues = await _unitOfWork.GetRepository<LeagueId>()
                .GetAll().Where(l => !l.IsDeleted)
                .ToListAsync();

            if (leagues == null || leagues.Count == 0)
            {
                return [];
            }

            var result = new List<object>();
            foreach (var league in leagues)
            {
                var players = await _unitOfWork.GetRepository<Player>()
                    .GetAll()
                    .Where(p => p.LeagueNumber == league.Id)
                    .OrderBy(p => p.Rank)
                    .Select(p => new
                    {
                        p.PlayerId,
                        p.FullName,
                        p.Wins,
                        p.Losses,
                        p.Draws,
                        p.Points,
                        p.MatchesPlayed,
                        p.Rank,
                        p.WinRate
                    })
                    .ToListAsync();

                result.Add(new
                {
                    LeagueId = league.Id,
                    LeagueName = league.Name,
                    LeagueDescription = league.Description,
                    LeagueType = league.TypeOfLeague,
                    league.IsFinished,
                    league.CreatedOn,
                    Players = players
                });
            }

            return result;
        }
    }
}