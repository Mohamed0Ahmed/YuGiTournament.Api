using YuGiTournament.Api.ApiResponses;
using YuGiTournament.Api.DTOs;
using YuGiTournament.Api.Models.ViewModels;

namespace YuGiTournament.Api.Services.Abstractions
{
    public interface IClassicMatchService
    {
        Task<IEnumerable<MatchViewModel>> GetAllMatchesAsync();
        Task<MatchViewModel?> GetMatchByIdAsync(int matchId);
        Task<ApiResponse> ResetMatchByIdAsync(int matchId);
        Task<ApiResponse> UpdateMatchResultAsync(int matchId, MatchResultDto resultDto);
        Task<IEnumerable<LeagueWithMatchesViewModel>> GetAllLeaguesWithMatchesAsync();
    }
} 