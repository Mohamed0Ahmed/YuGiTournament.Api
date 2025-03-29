using YuGiTournament.Api.ApiResponses;
using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Services.Abstractions
{
    public interface IPlayerService
    {
        Task<IEnumerable<Player>> GetAllPlayersAsync();
        Task<Player?> GetPlayerByIdAsync(int playerId);
        Task<ApiResponse> AddPlayerAsync(string NamePlayer);
        Task<ApiResponse> DeletePlayerAsync(int playerId);
        Task<IEnumerable<Player>> GetPlayersRankingAsync();
    }
}
