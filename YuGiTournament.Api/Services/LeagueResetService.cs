using Microsoft.EntityFrameworkCore;
using YuGiTournament.Api.Data;
using YuGiTournament.Api.Services.Abstractions;

namespace YuGiTournament.Api.Services
{
    public class LeagueResetService : ILeagueResetService
    {
        private readonly ApplicationDbContext _context;

        public LeagueResetService(ApplicationDbContext context) 
        {
            _context = context;
        }

        public async Task ResetLeagueAsync()
        {
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Matches");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM MatchRounds");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Players");

            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Matches', RESEED, 0)");
            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('MatchRounds', RESEED, 0)");
            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Players', RESEED, 0)");
        }
    }
}
