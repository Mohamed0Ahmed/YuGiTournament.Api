using Microsoft.EntityFrameworkCore;
using YuGiTournament.Api.Abstractions;
using YuGiTournament.Api.ApiResponses;
using YuGiTournament.Api.DTOs;
using YuGiTournament.Api.Models;
using YuGiTournament.Api.Services.Abstractions;

namespace YuGiTournament.Api.Services
{
    public class PlayerService : IPlayerService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPlayerRankingService _playerRankingService;

        public PlayerService(IUnitOfWork unitOfWork, IPlayerRankingService playerRankingService)
        {

            _unitOfWork = unitOfWork;
            _playerRankingService = playerRankingService;
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
                    .Include(m => m.Player1)
                    .Include(m => m.Player2)
                    .Include(m => m.Rounds)
                    .Where(m => m.Player1Id == playerId || m.Player2Id == playerId)
                    .ToListAsync();

                // Get league to check system
                var league = await _unitOfWork.GetRepository<LeagueId>().Find(l => l.Id == playerToDelete.LeagueNumber).FirstOrDefaultAsync();
                bool isClassic = league != null && league.SystemOfLeague == SystemOfLeague.Classic;

                foreach (var match in playerMatches)
                {
                    var opponent = match.Player1Id == playerId ? match.Player2 : match.Player1;
                    if (opponent == null)
                        continue;

                    if (isClassic)
                    {
                        // عكس احتساب النقاط للخصم في نظام Classic فقط إذا كانت المباراة مكتملة
                        if (match.IsCompleted)
                        {
                            if (match.Score1 == 1 && match.Score2 == 1)
                            {
                                // كانت تعادل
                                opponent.Draws -= 1;
                                opponent.Points -= 1;
                            }
                            else if (match.Score1 == 3 && match.Score2 == 0 && match.Player1Id != playerId)
                            {
                                // الخصم كان هو الفائز
                                opponent.Wins -= 1;
                                opponent.Points -= 3;
                            }
                            else if (match.Score2 == 3 && match.Score1 == 0 && match.Player2Id != playerId)
                            {
                                // الخصم كان هو الفائز
                                opponent.Wins -= 1;
                                opponent.Points -= 3;
                            }
                            else if ((match.Score1 == 3 && match.Score2 == 0 && match.Player1Id == playerId) || (match.Score2 == 3 && match.Score1 == 0 && match.Player2Id == playerId))
                            {
                                // الخصم كان هو الخاسر
                                opponent.Losses -= 1;
                            }
                            opponent.MatchesPlayed -= 1;
                            opponent.WinRate = opponent.MatchesPlayed > 0 ? (double)opponent.Wins / opponent.MatchesPlayed * 100 : 0;
                        }
                    }
                    else
                    {
                        // النظام القديم: اعتمد على MatchRounds
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

                var leaguePlayers = await _unitOfWork.GetRepository<Player>().GetAll().Where(p => p.LeagueNumber == playerToDelete.LeagueNumber).ToListAsync();
                var leagueMatches = await _unitOfWork.GetRepository<Match>().GetAll().Where(m => m.LeagueNumber == playerToDelete.LeagueNumber).ToListAsync();
                var rankedPlayers = _playerRankingService.RankPlayers(leaguePlayers, leagueMatches);
                foreach (var p in rankedPlayers) { var dbPlayer = leaguePlayers.First(x => x.PlayerId == p.PlayerId); dbPlayer.Rank = p.Rank; }
                await _unitOfWork.SaveChangesAsync();

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

            // إذا كانت البطولة من نوع مجموعات، أضف اللاعب فقط ولا تنشئ مباريات
            if (league.TypeOfLeague == LeagueType.Groups)
            {
                player.LeagueNumber = league.Id;
                await _unitOfWork.GetRepository<Player>().AddAsync(player);
                await _unitOfWork.SaveChangesAsync();
                return new ApiResponse(true, $"تم اضافة اللاعب {player.FullName} بنجاح في بطولة مجموعات. سيتم توزيع المجموعات وإنشاء المباريات لاحقًا.");
            }

            var ExistPlayer = await _unitOfWork.GetRepository<Player>().Find(x => x.FullName == fullName).Where(p => p.LeagueNumber == league.Id).FirstOrDefaultAsync();

            if (ExistPlayer != null)
                return new ApiResponse(false, $"تم اضافة اللاعب {player.FullName} من قبل !!!");

            player.LeagueNumber = league.Id;
            await _unitOfWork.GetRepository<Player>().AddAsync(player);
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
                    LeagueNumber = league.Id,
                    Stage = TournamentStage.League,
                });
            }

            if (matches.Count != 0)
            {
                await _unitOfWork.GetRepository<Match>().AddRangeAsync(matches);
                await _unitOfWork.SaveChangesAsync();
                var leaguePlayers = await _unitOfWork.GetRepository<Player>().GetAll().Where(p => p.LeagueNumber == league.Id).ToListAsync();
                var leagueMatches = await _unitOfWork.GetRepository<Match>().GetAll().Where(m => m.LeagueNumber == league.Id).ToListAsync();
                var rankedPlayers = _playerRankingService.RankPlayers(leaguePlayers, leagueMatches);
                foreach (var p in rankedPlayers) { var dbPlayer = leaguePlayers.First(x => x.PlayerId == p.PlayerId); dbPlayer.Rank = p.Rank; }
                await _unitOfWork.SaveChangesAsync();
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

        public async Task<IEnumerable<LeagueResponseDto>> GetAllLeaguesWithRankAsync()
        {
            var leagues = await _unitOfWork.GetRepository<LeagueId>()
                .GetAll().Where(l => !l.IsDeleted)
                .ToListAsync();

            if (leagues == null || !leagues.Any())
            {
                return new List<LeagueResponseDto>();
            }

            var result = new List<LeagueResponseDto>();
            foreach (var league in leagues)
            {
                var leagueResponse = new LeagueResponseDto
                {
                    LeagueId = league.Id,
                    LeagueName = league.Name,
                    LeagueDescription = league.Description,
                    LeagueType = league.TypeOfLeague,
                    SystemOfLeague = league.SystemOfLeague,
                    IsFinished = league.IsFinished,
                    CreatedOn = league.CreatedOn,
                    Players = league.TypeOfLeague != LeagueType.Groups ? await GetRankedPlayersForLeague(league.Id) : null,
                    Groups = league.TypeOfLeague == LeagueType.Groups ? await GetGroupedPlayersForLeague(league.Id) : null,
                    Matches = league.TypeOfLeague != LeagueType.Groups ? await GetMatchesForLeague(league.Id) : null,
                    KnockoutMatches = league.TypeOfLeague == LeagueType.Groups ? await GetKnockoutMatchesForLeague(league.Id) : null
                };

                result.Add(leagueResponse);
            }

            return result;
        }



        private async Task<List<PlayerRankDto>> GetRankedPlayersForLeague(int leagueId)
        {
            return await _unitOfWork.GetRepository<Player>()
                .GetAll()
                .Where(p => p.LeagueNumber == leagueId)
                .OrderBy(p => p.Rank)
                .Select(p => new PlayerRankDto
                {
                    PlayerId = p.PlayerId,
                    FullName = p.FullName,
                    Wins = p.Wins,
                    Losses = p.Losses,
                    Draws = p.Draws,
                    Points = p.Points,
                    MatchesPlayed = p.MatchesPlayed,
                    Rank = p.Rank,
                    WinRate = p.WinRate
                })
                .ToListAsync();
        }

        private async Task<List<GroupDto>> GetGroupedPlayersForLeague(int leagueId)
        {
            var players = await _unitOfWork.GetRepository<Player>()
                .GetAll()
                .Where(p => p.LeagueNumber == leagueId && p.GroupNumber != null && !p.IsDeleted)
                .OrderBy(p => p.Rank)
                .ToListAsync();

            var groups = players
                .GroupBy(p => p.GroupNumber)
                .OrderBy(g => g.Key)
                .Select(g => new GroupDto
                {
                    GroupNumber = g.Key ?? 0,
                    Players = g.Select(p => new PlayerRankDto
                    {
                        PlayerId = p.PlayerId,
                        FullName = p.FullName,
                        Wins = p.Wins,
                        Losses = p.Losses,
                        Draws = p.Draws,
                        Points = p.Points,
                        MatchesPlayed = p.MatchesPlayed,
                        Rank = p.Rank,
                        WinRate = p.WinRate
                    }).OrderBy(p => p.Rank).ToList()
                }).ToList();

            // إضافة المباريات لكل مجموعة
            foreach (var group in groups)
            {
                group.Matches = await GetGroupMatches(leagueId, group.GroupNumber);
            }

            return groups;
        }

        private async Task<List<MatchDto>> GetMatchesForLeague(int leagueId)
        {
            return await _unitOfWork.GetRepository<Match>()
                .GetAll()
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .Where(m => m.LeagueNumber == leagueId)
                .OrderBy(m => m.Stage)
                .ThenBy(m => m.MatchId)
                .Select(m => new MatchDto
                {
                    MatchId = m.MatchId,
                    Score1 = m.Score1,
                    Score2 = m.Score2,
                    IsCompleted = m.IsCompleted,
                    Player1Name = m.Player1.FullName,
                    Player2Name = m.Player2.FullName,
                    Player1Id = m.Player1Id,
                    Player2Id = m.Player2Id,
                    TournamentStage = m.Stage,
                    WinnerId = m.WinnerId
                })
                .ToListAsync();
        }

        private async Task<List<MatchDto>> GetGroupMatches(int leagueId, int groupNumber)
        {
            return await _unitOfWork.GetRepository<Match>()
                .GetAll()
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .Where(m => m.LeagueNumber == leagueId &&
                           m.Stage == TournamentStage.GroupStage &&
                           ((m.Player1.GroupNumber == groupNumber && m.Player2.GroupNumber == groupNumber) ||
                            (m.Player1.GroupNumber == groupNumber && m.Player2.GroupNumber == groupNumber)) &&
                           !m.IsDeleted)
                .OrderBy(m => m.MatchId)
                .Select(m => new MatchDto
                {
                    MatchId = m.MatchId,
                    Score1 = m.Score1,
                    Score2 = m.Score2,
                    IsCompleted = m.IsCompleted,
                    Player1Name = m.Player1.FullName,
                    Player2Name = m.Player2.FullName,
                    Player1Id = m.Player1Id,
                    Player2Id = m.Player2Id,
                    TournamentStage = m.Stage,
                    WinnerId = m.WinnerId
                })
                .ToListAsync();
        }

        private async Task<List<MatchDto>> GetKnockoutMatchesForLeague(int leagueId)
        {
            return await _unitOfWork.GetRepository<Match>()
                .GetAll()
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .Where(m => m.LeagueNumber == leagueId &&
                           m.Stage != TournamentStage.GroupStage &&
                           !m.IsDeleted)
                .OrderBy(m => m.Stage)
                .ThenBy(m => m.MatchId)
                .Select(m => new MatchDto
                {
                    MatchId = m.MatchId,
                    Score1 = m.Score1,
                    Score2 = m.Score2,
                    IsCompleted = m.IsCompleted,
                    Player1Name = m.Player1.FullName,
                    Player2Name = m.Player2.FullName,
                    Player1Id = m.Player1Id,
                    Player2Id = m.Player2Id,
                    TournamentStage = m.Stage,
                    WinnerId = m.WinnerId
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<GroupDto>> GetGroupedPlayersAsync(int leagueId)
        {
            var players = await _unitOfWork.GetRepository<Player>()
                .GetAll()
                .Where(p => p.LeagueNumber == leagueId && p.GroupNumber != null && !p.IsDeleted)
                .OrderBy(p => p.GroupNumber)
                .ToListAsync();

            if (!players.Any())
            {
                return new List<GroupDto>();
            }

            var groupedPlayers = players
                .GroupBy(p => p.GroupNumber)
                .Select(g => new GroupDto
                {
                    GroupNumber = g.Key ?? 0,
                    Players = g.Select(p => new PlayerRankDto
                    {
                        PlayerId = p.PlayerId,
                        FullName = p.FullName,
                        Wins = p.Wins,
                        Losses = p.Losses,
                        Draws = p.Draws,
                        Points = p.Points,
                        MatchesPlayed = p.MatchesPlayed,
                        Rank = p.Rank,
                        WinRate = p.WinRate
                    }).ToList()
                });

            return groupedPlayers;
        }
    }
}