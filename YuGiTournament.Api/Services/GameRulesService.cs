using Microsoft.EntityFrameworkCore;
using YuGiTournament.Api.Services.Abstractions;
using YuGiTournament.Api.Abstractions;
using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Services
{
    public class GameRulesService : IGameRulesService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public GameRulesService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<int> GetMaxRoundsPerMatchAsync()
        {
            var cfg = await _unitOfWork.GetRepository<GameRulesConfig>()
                .GetAll().OrderByDescending(c => c.UpdatedAt).FirstOrDefaultAsync();
            if (cfg != null && cfg.MaxRoundsPerMatch > 0)
            {
                return cfg.MaxRoundsPerMatch;
            }
            return _configuration.GetValue<int>("GameRules:MaxRoundsPerMatch", 5);
        }

        public async Task<GameRulesConfig> GetConfigAsync()
        {
            var cfg = await _unitOfWork.GetRepository<GameRulesConfig>()
                .GetAll().OrderByDescending(c => c.UpdatedAt).FirstOrDefaultAsync();
            if (cfg != null) return cfg;
            return new GameRulesConfig { MaxRoundsPerMatch = _configuration.GetValue<int>("GameRules:MaxRoundsPerMatch", 5) };
        }

        public async Task<GameRulesConfig> UpsertMaxRoundsPerMatchAsync(int maxRounds)
        {
            if (maxRounds <= 0) throw new ArgumentOutOfRangeException(nameof(maxRounds));

            var repo = _unitOfWork.GetRepository<GameRulesConfig>();
            var existing = await repo.GetAll().OrderByDescending(c => c.UpdatedAt).FirstOrDefaultAsync();
            if (existing == null)
            {
                existing = new GameRulesConfig { MaxRoundsPerMatch = maxRounds, UpdatedAt = DateTime.UtcNow };
                await repo.AddAsync(existing);
            }
            else
            {
                existing.MaxRoundsPerMatch = maxRounds;
                existing.UpdatedAt = DateTime.UtcNow;
                repo.Update(existing);
            }
            await _unitOfWork.SaveChangesAsync();
            return existing;
        }
    }
}


