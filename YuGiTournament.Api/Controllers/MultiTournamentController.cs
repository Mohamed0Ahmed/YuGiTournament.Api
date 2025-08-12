using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YuGiTournament.Api.DTOs;
using YuGiTournament.Api.Models;
using YuGiTournament.Api.Services.Abstractions;

namespace YuGiTournament.Api.Controllers
{
    [Route("api/multi")]
    [ApiController]
    public class MultiTournamentController : ControllerBase
    {
        private readonly IMultiTournamentService _service;

        public MultiTournamentController(IMultiTournamentService service)
        {
            _service = service;
        }

        #region Tournament Management (3 endpoints)

        /// <summary>
        /// Create a new tournament
        /// </summary>
        [HttpPost("tournaments")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTournament([FromBody] CreateTournamentDto request)
        {
            var result = await _service.CreateTournamentAsync(
                request.Name,
                request.SystemOfScoring,
                request.TeamCount,
                request.PlayersPerTeam);

            return Ok(result);
        }

        /// <summary>
        /// Update tournament status (Created -> Started -> Finished)
        /// </summary>
        [HttpPut("tournaments/{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTournamentStatus(int id, [FromBody] UpdateTournamentStatusDto request)
        {
            var result = await _service.UpdateTournamentStatusAsync(id, request.Status);
            return Ok(result);
        }

        /// <summary>
        /// Delete tournament and all related data
        /// </summary>
        [HttpDelete("tournaments/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTournament(int id)
        {
            var result = await _service.DeleteTournamentAsync(id);
            return Ok(result);
        }

        #endregion

        #region Team Management (2 endpoints)

        /// <summary>
        /// Create a team with players for a tournament
        /// </summary>
        [HttpPost("tournaments/{tournamentId}/teams")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTeam(int tournamentId, [FromBody] TeamCreateDto request)
        {
            var result = await _service.CreateTeamAsync(tournamentId, request.TeamName, request.PlayerIds);
            return Ok(result);
        }

        /// <summary>
        /// Update team name and/or players (only in Created status)
        /// Replace player (only in Started status)
        /// </summary>
        [HttpPut("teams/{teamId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTeam(int teamId, [FromBody] TeamUpdateDto request)
        {
            // Check if this is a player replacement request
            if (request.TeamName == null && request.PlayerIds?.Count == 2)
            {
                // Assuming first ID is replaced player, second is new player
                var replaceRequest = new PlayerReplaceDto(request.PlayerIds[0], request.PlayerIds[1]);
                var result = await _service.ReplacePlayerAsync(teamId, replaceRequest.ReplacedPlayerId, replaceRequest.NewPlayerId);
                return Ok(result);
            }
            else
            {
                // Regular team update
                var result = await _service.UpdateTeamAsync(teamId, request.TeamName, request.PlayerIds);
                return Ok(result);
            }
        }

        #endregion

        #region Match & Results (2 endpoints)

        /// <summary>
        /// Get tournament details by ID
        /// </summary>
        [HttpGet("tournaments/{id}")]
        public async Task<IActionResult> GetTournamentData(int id)
        {
            var result = await _service.GetTournamentByIdAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Record match result
        /// </summary>
        [HttpPost("matches/{matchId}/result")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RecordMatchResult(int matchId, [FromBody] MultiMatchResultDto request)
        {
            var result = await _service.RecordMatchResultAsync(
                matchId,
                request.Score1,
                request.Score2,
                request.TotalPoints1,
                request.TotalPoints2);

            return Ok(result);
        }

        #endregion

        #region General Data (3 endpoints)

        /// <summary>
        /// Get active tournament
        /// </summary>
        [HttpGet("tournaments/active")]
        public async Task<IActionResult> GetActiveTournament()
        {
            var result = await _service.GetActiveTournamentAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get all tournaments (history)
        /// </summary>
        [HttpGet("tournaments")]
        public async Task<IActionResult> GetAllTournaments()
        {
            var result = await _service.GetAllTournamentsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get tournament standings/rankings
        /// </summary>
        [HttpGet("tournaments/{id}/standings")]
        public async Task<IActionResult> GetTournamentStandings(int id)
        {
            var result = await _service.GetTournamentStandingsAsync(id);
            return Ok(result);
        }

        #endregion
    }

    #region Request DTOs (moved from separate file for simplicity)

    public record CreateTournamentRequest(string Name, SystemOfLeague SystemOfScoring, int TeamCount, int PlayersPerTeam);
    public record TeamCreateRequest(string TeamName, List<int> PlayerIds);
    public record TeamUpdateRequest(string? TeamName, List<int>? PlayerIds);
    public record MultiMatchResultRequest(int? Score1, int? Score2, double? TotalPoints1, double? TotalPoints2);
    public record PlayerReplaceRequest(int ReplacedPlayerId, int NewPlayerId);
    public record UpdateTournamentStatusRequest(TournamentStatus Status);

    #endregion
}
