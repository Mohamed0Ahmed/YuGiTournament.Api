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
            return await _unitOfWork.GetRepository<Player>().GetAll().ToListAsync();
        }

        public async Task<Player?> GetPlayerByIdAsync(int playerId)
        {
            return await _unitOfWork.GetRepository<Player>().Find(p => p.PlayerId == playerId).FirstOrDefaultAsync();
        }

        public async Task<ApiResponse> DeletePlayerAsync(int playerId)
        {

            var player = await _unitOfWork.GetRepository<Player>().Find(player => player.PlayerId == playerId).FirstOrDefaultAsync();
            if (player == null)
                return new ApiResponse(false, "مفيش هنا لاعب بالاسم ده");

            var playerMatches = await _unitOfWork.GetRepository<Match>()
                .GetAll()
                .Where(m => m.Player1Id == playerId || m.Player2Id == playerId)
                .Select(m => m.MatchId)
                .ToListAsync();

            var roundsToDelete = _unitOfWork.GetRepository<MatchRound>()
                .GetAll()
                .Where(r => playerMatches.Contains(r.MatchId));
            _unitOfWork.GetRepository<MatchRound>().DeleteRange(roundsToDelete);

            var matchesToDelete = _unitOfWork.GetRepository<Match>()
                .GetAll()
                .Where(m => m.Player1Id == playerId || m.Player2Id == playerId);
            _unitOfWork.GetRepository<Match>().DeleteRange(matchesToDelete);

            _unitOfWork.GetRepository<Player>().Delete(player);

            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse(true, $"تم حذف اللاعب {player.FullName} وكل مبارياته والجولات المرتبطة بيه.");
        }

        public async Task<ApiResponse> AddPlayerAsync(string fullName)
        {
            var player = new Player { FullName = fullName };

            var ExistPlayer = await _unitOfWork.GetRepository<Player>().Find(x => x.FullName == fullName).FirstOrDefaultAsync();

            if (ExistPlayer != null)
                return new ApiResponse(false, $"تم اضافة اللاعب {player.FullName} من قبل !!!");

            await _unitOfWork.GetRepository<Player>().AddAsync(player);
            await _unitOfWork.SaveChangesAsync();

            var otherPlayers = await _unitOfWork.GetRepository<Player>()
                .GetAll()
                .Where(p => p.PlayerId != player.PlayerId)
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
                    IsCompleted = false
                });
            }

            if (matches.Count != 0)
            {
                await _unitOfWork.GetRepository<Match>().AddRangeAsync(matches);
                await _unitOfWork.SaveChangesAsync();
            }

            return new ApiResponse(true, $"تم اضافة اللاعب {player.FullName} وكل مبارياته والجولات المرتبطة بيه.");
        }


        public async Task<IEnumerable<Player>> GetPlayersRankingAsync()
        {

            var players = await _unitOfWork.GetRepository<Player>()
                .GetAll()
                .ToListAsync();


            var completedMatches = await _unitOfWork.GetRepository<Match>()
                .GetAll()
                .Where(m => m.IsCompleted)
                .ToListAsync();


            var rankedPlayers = players
                .OrderByDescending(p => p.Points)
                .ThenBy(p => p.PlayerId)
                .ToList();


            int currentRank = 1;
            for (int i = 0; i < rankedPlayers.Count; i++)
            {
                if (i > 0 && rankedPlayers[i].Points == rankedPlayers[i - 1].Points)
                {
                    var headToHead = completedMatches
                        .FirstOrDefault(m =>
                            (m.Player1Id == rankedPlayers[i].PlayerId && m.Player2Id == rankedPlayers[i - 1].PlayerId) ||
                            (m.Player2Id == rankedPlayers[i].PlayerId && m.Player1Id == rankedPlayers[i - 1].PlayerId));

                    if (headToHead != null)
                    {
                        if (headToHead.Score1 == headToHead.Score2)
                        {
                            if (rankedPlayers[i].Wins == rankedPlayers[i - 1].Wins)
                            {
                                rankedPlayers[i].Rank = rankedPlayers[i - 1].Rank;
                            }
                            else
                            {
                                rankedPlayers[i].Rank = currentRank;
                            }
                        }
                        else
                        {
                            rankedPlayers[i].Rank = currentRank;
                        }
                    }
                    else
                    {
                        if (rankedPlayers[i].Wins == rankedPlayers[i - 1].Wins)
                        {
                            rankedPlayers[i].Rank = rankedPlayers[i - 1].Rank;
                        }
                        else
                        {
                            rankedPlayers[i].Rank = currentRank;
                        }
                    }
                }
                else
                {
                    rankedPlayers[i].Rank = currentRank;
                }
                currentRank++;
            }

            return rankedPlayers;
        }

      
    }
}