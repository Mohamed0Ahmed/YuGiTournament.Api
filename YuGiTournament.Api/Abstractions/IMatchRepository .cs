using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Abstractions
{
    public interface IMatchRepository : IGenericRepository<Match>
    {
        Task<IEnumerable<Match>> GetMatchesByPlayerIdAsync(int playerId);
    }
}
