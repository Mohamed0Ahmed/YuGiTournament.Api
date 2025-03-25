using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Services.Abstractions
{
    public interface IPlayerService
    {
        Task<IEnumerable<Player>> GetAllPlayersAsync();
        Task<Player?> GetPlayerByIdAsync(int playerId);
        Task<Player> AddPlayerAsync(string NamePlayer);
        Task<bool> DeletePlayerAsync(int playerId);
    }
}
