using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Services.Abstractions
{
    public interface IGameRulesService
    {
        Task<int> GetMaxRoundsPerMatchAsync();
        Task<GameRulesConfig> GetConfigAsync();
        Task<GameRulesConfig> UpsertMaxRoundsPerMatchAsync(int maxRounds);
    }
}


