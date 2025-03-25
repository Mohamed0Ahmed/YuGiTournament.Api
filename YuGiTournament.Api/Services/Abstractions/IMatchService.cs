using YuGiTournament.Api.ApiResponses;
using YuGiTournament.Api.DTOs;
using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Services.Abstractions
{
    public interface IMatchService
    {
        Task<IEnumerable<object>> GetAllMatchesAsync();
        Task<object?> GetMatchByIdAsync(int matchId);

        Task DeleteMatchAsync(int matchId);
        Task<Match> CreateMatchAsync(int player1Id, int player2Id);
        Task<ApiResponse> UpdateMatchResultAsync(int matchId, MatchResultDto result);

    }
}
