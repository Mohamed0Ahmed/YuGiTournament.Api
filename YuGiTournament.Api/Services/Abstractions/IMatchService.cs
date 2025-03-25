using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Services.Abstractions
{
    public interface IMatchService
    {
        Task<IEnumerable<object>> GetAllMatchesAsync();
        Task<object?> GetMatchByIdAsync(int matchId);
        Task AddMatchAsync(Match match);
        Task DeleteMatchAsync(int matchId);
        Task<Match> CreateMatchAsync(int player1Id, int player2Id);
        Task<bool> UpdateMatchResultAsync(int matchId, int? winnerId);

    }
}
