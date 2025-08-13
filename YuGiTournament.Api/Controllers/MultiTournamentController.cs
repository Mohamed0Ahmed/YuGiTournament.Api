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

        #region Match & Results (3 endpoints)

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
        /// Get tournament matches/fixtures
        /// </summary>
        [HttpGet("tournaments/{id}/matches")]
        public async Task<IActionResult> GetTournamentMatches(int id)
        {
            var result = await _service.GetTournamentMatchesAsync(id);
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
                request.WinnerId,
                request.Score1,
                request.Score2);

            return Ok(result);
        }

        /// <summary>
        /// Undo match result (reset to 0-0, IsCompleted = false)
        /// </summary>
        [HttpPost("matches/{matchId}/undo")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UndoMatchResult(int matchId)
        {
            var result = await _service.UndoMatchResultAsync(matchId);
            return Ok(result);
        }

        /// <summary>
        /// Get all matches for a specific player in active tournament
        /// </summary>
        [HttpGet("players/{playerId}/matches")]
        public async Task<IActionResult> GetPlayerMatches(int playerId)
        {
            var result = await _service.GetPlayerMatchesAsync(playerId);
            return Ok(result);
        }

        #endregion

        #region Player Management (3 endpoints)

        /// <summary>
        /// Get all available players for team creation
        /// </summary>
        [HttpGet("players")]
        public async Task<IActionResult> GetAllPlayers()
        {
            var result = await _service.GetAllPlayersAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get specific player by ID
        /// </summary>
        [HttpGet("players/{playerId}")]
        public async Task<IActionResult> GetPlayerById(int playerId)
        {
            var result = await _service.GetPlayerByIdAsync(playerId);
            return Ok(result);
        }

        /// <summary>
        /// Add new player to the system
        /// </summary>
        [HttpPost("players")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddPlayer([FromBody] AddPlayerDto request)
        {
            var result = await _service.AddPlayerAsync(request.FullName);
            return Ok(result);
        }

        #endregion

        #region Helper Endpoints (3 endpoints)

        /// <summary>
        /// Start tournament (helper method)
        /// </summary>
        [HttpPost("tournaments/{id}/start")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> StartTournament(int id)
        {
            var result = await _service.UpdateTournamentStatusAsync(id, TournamentStatus.Started);
            return Ok(result);
        }

        /// <summary>
        /// Finish tournament (helper method)
        /// </summary>
        [HttpPost("tournaments/{id}/finish")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> FinishTournament(int id)
        {
            var result = await _service.UpdateTournamentStatusAsync(id, TournamentStatus.Finished);
            return Ok(result);
        }

        /// <summary>
        /// Replace player in team (helper method)
        /// </summary>
        [HttpPost("teams/{teamId}/replace-player")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReplacePlayer(int teamId, [FromBody] PlayerReplaceDto request)
        {
            var result = await _service.ReplacePlayerAsync(teamId, request.ReplacedPlayerId, request.NewPlayerId);
            return Ok(result);
        }

        #endregion

        #region General Data (4 endpoints)

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
        /// Get matches for the active tournament
        /// </summary>
        [HttpGet("tournaments/active/matches")]
        public async Task<IActionResult> GetActiveTournamentMatches()
        {
            var result = await _service.GetActiveTournamentMatchesAsync();
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
    public record MultiMatchResultRequest(double? Score1, double? Score2, double? TotalPoints1, double? TotalPoints2);
    public record PlayerReplaceRequest(int ReplacedPlayerId, int NewPlayerId);
    public record UpdateTournamentStatusRequest(TournamentStatus Status);

    #endregion
}
