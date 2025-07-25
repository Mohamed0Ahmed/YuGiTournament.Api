using YuGiTournament.Api.ApiResponses;
using YuGiTournament.Api.DTOs;

namespace YuGiTournament.Api.Services.Abstractions
{
    public interface ILeagueResetService
    {
        Task<LeagueResponse> GetCurrentLeague();
        Task<ApiResponse> ResetLeagueAsync(int leagueId);
        Task<ApiResponse> DeleteLeague(int leagueId);
        Task<ApiResponse> StartLeagueAsync(StartLeagueDto newLeague);
        Task<ApiResponse> CreateGroupsAndMatchesAsync(int leagueId);
        Task<ApiResponse> StartKnockoutStageAsync(int leagueId);
    }
}

