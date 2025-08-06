using YuGiTournament.Api.ApiResponses;
using YuGiTournament.Api.DTOs;
using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Abstractions
{
    public interface IFriendlyMatchService
    {
        // Player Management
        Task<IEnumerable<FriendlyPlayerDto>> GetAllFriendlyPlayersAsync();
        Task<FriendlyPlayerDto?> GetFriendlyPlayerByIdAsync(int playerId);
        Task<ApiResponse> AddFriendlyPlayerAsync(string fullName);
        Task<ApiResponse> DeleteFriendlyPlayerAsync(int playerId);
        Task<ApiResponse> DeactivateFriendlyPlayerAsync(int playerId);

        // Match Management
        Task<IEnumerable<FriendlyMatchHistoryDto>> GetAllFriendlyMatchesAsync();
        Task<IEnumerable<FriendlyMatchHistoryDto>> GetFriendlyMatchesBetweenPlayersAsync(int player1Id, int player2Id);
        Task<ApiResponse> RecordFriendlyMatchAsync(RecordFriendlyMatchDto matchDto);
        Task<ApiResponse> DeleteFriendlyMatchAsync(int matchId);
        Task<ApiResponse> UpdateFriendlyMatchAsync(int matchId, RecordFriendlyMatchDto matchDto);

        // Advanced Match Filtering and Pagination
        Task<PaginatedMatchResultDto> GetFilteredMatchesAsync(MatchFilterDto filter);
        Task<PaginatedMatchResultDto> GetPlayerMatchesAsync(int playerId, MatchFilterDto filter);
        Task<PaginatedMatchResultDto> GetPlayerVsOpponentsMatchesAsync(int playerId, List<int> opponentIds, MatchFilterDto filter);

        // Statistics and Analysis
        Task<OverallScoreDto?> GetOverallScoreBetweenPlayersAsync(int player1Id, int player2Id);
        Task<FriendlyPlayerDto?> GetFriendlyPlayerStatsAsync(int playerId);
        Task<IEnumerable<FriendlyPlayerDto>> GetFriendlyPlayersRankingAsync();
        Task<IEnumerable<PlayerStatisticsDto>> GetPlayersStatisticsAsync();

        // Shutout Results (5-0 matches)
        Task<IEnumerable<ShutoutResultDto>> GetAllShutoutResultsAsync();
        Task<IEnumerable<ShutoutResultDto>> GetShutoutResultsByPlayerAsync(int playerId);
        Task<ShutoutResultDto?> GetShutoutResultByMatchAsync(int matchId);
        Task<ApiResponse> DeleteShutoutResultAsync(int shutoutId);

        // Advanced Shutout Filtering and Pagination
        Task<PaginatedShutoutResultDto> GetFilteredShutoutsAsync(ShutoutFilterDto filter);
        Task<PaginatedShutoutResultDto> GetPlayerShutoutsAsync(int playerId, ShutoutFilterDto filter);
        Task<PaginatedShutoutResultDto> GetPlayersShutoutsAsync(List<int> playerIds, ShutoutFilterDto filter);
    }
}