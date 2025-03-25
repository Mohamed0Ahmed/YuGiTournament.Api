using YuGiTournament.Api.ApiResponses;
using YuGiTournament.Api.DTOs;
using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Services.Abstractions
{
    public interface IMatchService
    {
        Task<IEnumerable<object>> GetAllMatchesAsync();
        Task<object?> GetMatchByIdAsync(int matchId);
        Task<ApiResponse> ResetMatchByIdAsync(int matchId);
        Task<ApiResponse> UpdateMatchResultAsync(int matchId, MatchResultDto resultDto);
    }
}
