using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using YuGiTournament.Api.Abstractions;
using YuGiTournament.Api.ApiResponses;
using YuGiTournament.Api.DTOs;

namespace YuGiTournament.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FriendlyMatchController : ControllerBase
    {
        private readonly IFriendlyMatchService _friendlyMatchService;

        public FriendlyMatchController(IFriendlyMatchService friendlyMatchService)
        {
            _friendlyMatchService = friendlyMatchService;
        }

        #region Player Management

        [HttpGet("players")]
        public async Task<IActionResult> GetAllFriendlyPlayers()
        {
            var players = await _friendlyMatchService.GetAllFriendlyPlayersAsync();
            return Ok(players);
        }

        [HttpGet("players/{playerId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetFriendlyPlayerById(int playerId)
        {
            var player = await _friendlyMatchService.GetFriendlyPlayerByIdAsync(playerId);
            if (player == null)
            {
                return NotFound(new ApiResponse(false, "اللاعب غير موجود"));
            }

            return Ok(player);
        }

        [HttpPost("players")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddFriendlyPlayer([FromBody] AddFriendlyPlayerDto dto)
        {
            var response = await _friendlyMatchService.AddFriendlyPlayerAsync(dto.FullName);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpDelete("players/{playerId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteFriendlyPlayer(int playerId)
        {
            var response = await _friendlyMatchService.DeleteFriendlyPlayerAsync(playerId);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPut("players/{playerId}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateFriendlyPlayer(int playerId)
        {
            var response = await _friendlyMatchService.DeactivateFriendlyPlayerAsync(playerId);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpGet("players/ranking")]
        public async Task<IActionResult> GetFriendlyPlayersRanking()
        {
            var players = await _friendlyMatchService.GetFriendlyPlayersRankingAsync();
            return Ok(players);
        }

        [HttpGet("players/{playerId}/stats")]
        public async Task<IActionResult> GetFriendlyPlayerStats(int playerId)
        {
            var player = await _friendlyMatchService.GetFriendlyPlayerStatsAsync(playerId);
            if (player == null)
            {
                return NotFound(new ApiResponse(false, "اللاعب غير موجود"));
            }

            return Ok(player);
        }

        #endregion

        #region Match Management

        [HttpGet("matches")]
        public async Task<IActionResult> GetAllFriendlyMatches()
        {
            var matches = await _friendlyMatchService.GetAllFriendlyMatchesAsync();
            return Ok(matches);
        }

        [HttpGet("matches/{player1Id}/{player2Id}")]
        public async Task<IActionResult> GetFriendlyMatchesBetweenPlayers(int player1Id, int player2Id)
        {
            var matches = await _friendlyMatchService.GetFriendlyMatchesBetweenPlayersAsync(player1Id, player2Id);
            return Ok(matches);
        }

        [HttpPost("matches/record")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RecordFriendlyMatch([FromBody] RecordFriendlyMatchDto dto)
        {
            var response = await _friendlyMatchService.RecordFriendlyMatchAsync(dto);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpDelete("matches/{matchId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteFriendlyMatch(int matchId)
        {
            var response = await _friendlyMatchService.DeleteFriendlyMatchAsync(matchId);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPut("matches/{matchId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateFriendlyMatch(int matchId, [FromBody] RecordFriendlyMatchDto dto)
        {
            var response = await _friendlyMatchService.UpdateFriendlyMatchAsync(matchId, dto);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        #endregion

        #region Statistics and Analysis

        [HttpGet("overall-score/{player1Id}/{player2Id}")]
        public async Task<IActionResult> GetOverallScoreBetweenPlayers(int player1Id, int player2Id)
        {
            var overallScore = await _friendlyMatchService.GetOverallScoreBetweenPlayersAsync(player1Id, player2Id);
            if (overallScore == null)
            {
                return NotFound(new ApiResponse(false, "أحد اللاعبين غير موجود"));
            }

            return Ok(overallScore);
        }

        [HttpGet("players/statistics")]
        public async Task<IActionResult> GetPlayersStatistics()
        {
            var statistics = await _friendlyMatchService.GetPlayersStatisticsAsync();
            return Ok(statistics);
        }

        #endregion

        #region Shutout Results (5-0 matches)

        [HttpGet("shutouts")]
        public async Task<IActionResult> GetAllShutoutResults()
        {
            var shutouts = await _friendlyMatchService.GetAllShutoutResultsAsync();
            return Ok(shutouts);
        }

        [HttpGet("shutouts/player/{playerId}")]
        public async Task<IActionResult> GetShutoutResultsByPlayer(int playerId)
        {
            var shutouts = await _friendlyMatchService.GetShutoutResultsByPlayerAsync(playerId);
            return Ok(shutouts);
        }

        [HttpGet("shutouts/match/{matchId}")]
        public async Task<IActionResult> GetShutoutResultByMatch(int matchId)
        {
            var shutout = await _friendlyMatchService.GetShutoutResultByMatchAsync(matchId);
            if (shutout == null)
            {
                return NotFound(new ApiResponse(false, "لا توجد نتيجة عريضة لهذه المباراة"));
            }

            return Ok(shutout);
        }

        [HttpDelete("shutouts/{shutoutId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteShutoutResult(int shutoutId)
        {
            var response = await _friendlyMatchService.DeleteShutoutResultAsync(shutoutId);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        #endregion

        #region Advanced Shutout Filtering and Pagination

        [HttpGet("shutouts/filtered")]
        public async Task<IActionResult> GetFilteredShutouts([FromQuery] ShutoutFilterDto filter)
        {
            var result = await _friendlyMatchService.GetFilteredShutoutsAsync(filter);
            return Ok(result);
        }

        [HttpGet("shutouts/player/{playerId}/filtered")]
        public async Task<IActionResult> GetPlayerShutouts(int playerId, [FromQuery] ShutoutFilterDto filter)
        {
            var result = await _friendlyMatchService.GetPlayerShutoutsAsync(playerId, filter);
            if (result.TotalShutouts == 0 && result.FilterSummary == "اللاعب غير موجود")
            {
                return NotFound(new { Message = "اللاعب غير موجود" });
            }
            return Ok(result);
        }

        [HttpPost("shutouts/players/filtered")]
        public async Task<IActionResult> GetPlayersShutouts([FromBody] List<int> playerIds, [FromQuery] ShutoutFilterDto filter)
        {
            if (playerIds == null || !playerIds.Any())
            {
                return BadRequest(new { Message = "يجب تحديد لاعب واحد على الأقل" });
            }

            var result = await _friendlyMatchService.GetPlayersShutoutsAsync(playerIds, filter);
            if (result.TotalShutouts == 0 && result.FilterSummary == "اللاعبين غير موجودين")
            {
                return NotFound(new { Message = "اللاعبين غير موجودين" });
            }
            return Ok(result);
        }

        #endregion

        #region Advanced Match Filtering and Pagination

        [HttpGet("matches/filtered")]
        public async Task<IActionResult> GetFilteredMatches([FromQuery] MatchFilterDto filter)
        {
            var result = await _friendlyMatchService.GetFilteredMatchesAsync(filter);
            return Ok(result);
        }

        [HttpGet("matches/player/{playerId}")]
        public async Task<IActionResult> GetPlayerMatches(int playerId, [FromQuery] MatchFilterDto filter)
        {
            var result = await _friendlyMatchService.GetPlayerMatchesAsync(playerId, filter);
            if (result.TotalMatches == 0 && result.FilterSummary == "اللاعب غير موجود")
            {
                return NotFound(new { Message = "اللاعب غير موجود" });
            }
            return Ok(result);
        }

        [HttpPost("matches/player/{playerId}/vs-opponents")]
        public async Task<IActionResult> GetPlayerVsOpponentsMatches(int playerId, [FromBody] List<int> opponentIds, [FromQuery] MatchFilterDto filter)
        {
            if (opponentIds == null || !opponentIds.Any())
            {
                return BadRequest(new { Message = "يجب تحديد لاعب واحد على الأقل من المنافسين" });
            }

            var result = await _friendlyMatchService.GetPlayerVsOpponentsMatchesAsync(playerId, opponentIds, filter);
            if (result.TotalMatches == 0 && result.FilterSummary == "اللاعب غير موجود")
            {
                return NotFound(new { Message = "اللاعب غير موجود" });
            }
            return Ok(result);
        }

        #endregion

    }
}