using YuGiTournament.Api.ApiResponses;
using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Services.Abstractions
{
    public interface IMultiTournamentService
    {
        #region New Simplified Methods

        // Tournament Management
        Task<ApiResponse> CreateTournamentAsync(string name, SystemOfLeague systemOfScoring, int teamCount, int playersPerTeam);
        Task<ApiResponse> UpdateTournamentStatusAsync(int tournamentId, TournamentStatus status);
        Task<ApiResponse> DeleteTournamentAsync(int tournamentId);

        // Team Management
        Task<ApiResponse> CreateTeamAsync(int tournamentId, string teamName, List<int> playerIds);
        Task<ApiResponse> UpdateTeamAsync(int teamId, string? teamName, List<int>? playerIds);
        Task<ApiResponse> ReplacePlayerAsync(int teamId, int replacedPlayerId, int newPlayerId);

        // Match Management
        Task<ApiResponse> RecordMatchResultAsync(int matchId, int? winnerId, double? score1, double? score2);
        Task<ApiResponse> UndoMatchResultAsync(int matchId);
        Task<ApiResponse> GetPlayerMatchesAsync(int playerId);

        // Data Retrieval
        Task<ApiResponse> GetActiveTournamentAsync();
        Task<ApiResponse> GetTournamentByIdAsync(int tournamentId);
        Task<ApiResponse> GetAllTournamentsAsync();
        Task<ApiResponse> GetTournamentMatchesAsync(int tournamentId);
        Task<ApiResponse> GetActiveTournamentMatchesAsync();
        Task<ApiResponse> GetTournamentStandingsAsync(int tournamentId);

        // Player Management
        Task<ApiResponse> GetAllPlayersAsync();
        Task<ApiResponse> GetPlayerByIdAsync(int playerId);
        Task<ApiResponse> AddPlayerAsync(string fullName);

        #endregion
    }
}