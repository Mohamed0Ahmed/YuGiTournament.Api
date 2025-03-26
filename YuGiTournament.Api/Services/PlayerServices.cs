using Microsoft.EntityFrameworkCore;
using YuGiTournament.Api.Abstractions;
using YuGiTournament.Api.ApiResponses;
using YuGiTournament.Api.Data;
using YuGiTournament.Api.Models;
using YuGiTournament.Api.Services.Abstractions;

namespace YuGiTournament.Api.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly ApplicationDbContext _context;

        public PlayerService(IPlayerRepository playerRepository, ApplicationDbContext context)
        {
            _playerRepository = playerRepository;
            _context = context;
        }

        public async Task<IEnumerable<Player>> GetAllPlayersAsync()
        {
            return await _playerRepository.GetAllAsync();
        }

        public async Task<Player?> GetPlayerByIdAsync(int playerId)
        {
            return await _playerRepository.GetByIdAsync(playerId);
        }

        public async Task<ApiResponse> DeletePlayerAsync(int playerId)
        {
            var player = await _context.Players.FindAsync(playerId);
            if (player == null)
                return new ApiResponse("مفيش هنا لاعب بالاسم ده");

            var playerMatches = await _context.Matches
                .Where(m => m.Player1Id == playerId || m.Player2Id == playerId)
                .Select(m => m.MatchId)
                .ToListAsync();

            var roundsToDelete = _context.MatchRounds
                .Where(r => playerMatches.Contains(r.MatchId));
            _context.MatchRounds.RemoveRange(roundsToDelete);

            var matchesToDelete = _context.Matches
                .Where(m => m.Player1Id == playerId || m.Player2Id == playerId);
            _context.Matches.RemoveRange(matchesToDelete);

            _playerRepository.Delete(player);

            await _context.SaveChangesAsync();
            return new ApiResponse($"تم حذف اللاعب {player.FullName} وكل مبارياته والجولات المرتبطة بيه.");
        }

        public async Task<Player> AddPlayerAsync(string fullName)
        {
            var player = new Player { FullName = fullName };

            _context.Players.Add(player);
            await _context.SaveChangesAsync(); 

            var otherPlayers = await _context.Players
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
                _context.Matches.AddRange(matches);
                await _context.SaveChangesAsync();
            }

            return player;
        }

    }
}
