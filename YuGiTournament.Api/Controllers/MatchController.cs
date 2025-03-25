using Microsoft.AspNetCore.Mvc;
using YuGiTournament.Api.Services.Abstractions;
using YuGiTournament.Api.Models;
using YuGiTournament.Api.DTOs;

namespace YuGiTournament.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MatchController : ControllerBase
    {
        private readonly IMatchService _matchService;

        public MatchController(IMatchService matchService)
        {
            _matchService = matchService;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Match>>> GetMatches()
        {
            var matches = await _matchService.GetAllMatchesAsync();
            return Ok(matches);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Match>> GetMatch(int id)
        {
            var match = await _matchService.GetMatchByIdAsync(id);
            if (match == null)
            {
                return NotFound("Match not found.");
            }
            return Ok(match);
        }


        
        [HttpPost("{matchId}/result")]
        public async Task<IActionResult> UpdateMatchResult(int matchId, [FromBody] MatchResultDto resultDto)
        {
            var success = await _matchService.UpdateMatchResultAsync(matchId, resultDto.WinnerId);

            if (!success)
                return BadRequest("Failed to update match result or match already completed.");

            return Ok("Match result updated successfully.");
        }

    }


}
