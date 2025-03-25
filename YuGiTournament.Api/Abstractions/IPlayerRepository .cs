using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Abstractions
{
    public interface IPlayerRepository : IGenericRepository<Player>
    {
        Task<Player?> GetPlayerWithMatchesAsync(int playerId);
    }
}
