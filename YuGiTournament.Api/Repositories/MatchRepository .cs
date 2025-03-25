using Microsoft.EntityFrameworkCore;
using YuGiTournament.Api.Abstractions;
using YuGiTournament.Api.Data;
using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Repositories
{
    public class MatchRepository : GenericRepository<Match>, IMatchRepository
    {
        public MatchRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Match>> GetMatchesByPlayerIdAsync(int playerId)
        {
            return await _context.Matches
                .Where(m => m.Player1Id == playerId || m.Player2Id == playerId)
                .ToListAsync();
        }
    }
}
