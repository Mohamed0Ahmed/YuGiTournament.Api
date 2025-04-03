using YuGiTournament.Api.ApiResponses;
using YuGiTournament.Api.DTOs;

namespace YuGiTournament.Api.Services.Abstractions
{
    public interface ILeagueResetService
    {
        Task<LeagueResponse> GetCurrentLeague();
        Task<ApiResponse> ResetLeagueAsync(int leagueId);
        Task<ApiResponse> StartLeagueAsync(StartLeagueDto newLeague);
    }
}

