using Microsoft.EntityFrameworkCore;
using YuGiTournament.Api.Abstractions;
using YuGiTournament.Api.Data;
using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Repositories
{
    public class PlayerRepository : GenericRepository<Player>, IPlayerRepository
    {
        public PlayerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Player?> GetPlayerWithMatchesAsync(int playerId)
        {
            return await _context.Players
                .Include(p => p.Matches) 
                .FirstOrDefaultAsync(p => p.PlayerId == playerId);
        }
    }
}
