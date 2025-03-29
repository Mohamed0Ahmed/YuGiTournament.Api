using Microsoft.EntityFrameworkCore;
using YuGiTournament.Api.Services.Abstractions;
using YuGiTournament.Api.Abstractions;

namespace YuGiTournament.Api.Services
{
    public class LeagueResetService : ILeagueResetService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LeagueResetService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task ResetLeagueAsync()
        {
            await _unitOfWork.GetDbContext().Database.ExecuteSqlRawAsync("DELETE FROM Matches");
            await _unitOfWork.GetDbContext().Database.ExecuteSqlRawAsync("DELETE FROM MatchRounds");
            await _unitOfWork.GetDbContext().Database.ExecuteSqlRawAsync("DELETE FROM Players");
            await _unitOfWork.GetDbContext().Database.ExecuteSqlRawAsync("DELETE FROM Messages");

            await _unitOfWork.GetDbContext().Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Matches', RESEED, 0)");
            await _unitOfWork.GetDbContext().Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('MatchRounds', RESEED, 0)");
            await _unitOfWork.GetDbContext().Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Players', RESEED, 0)");
            await _unitOfWork.GetDbContext().Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Messages', RESEED, 0)");
        }
    }
}

