using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YuGiTournament.Api.DTOs;
using YuGiTournament.Api.Services.Abstractions;

namespace YuGiTournament.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeagueController : ControllerBase
    {
        private readonly ILeagueResetService _leagueResetService;

        public LeagueController(ILeagueResetService leagueResetService)
        {
            _leagueResetService = leagueResetService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("reset/{leagueId}")]
        public async Task<IActionResult> ResetLeague(int leagueId)
        {
            var result = await _leagueResetService.ResetLeagueAsync(leagueId);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{leagueId}")]
        public async Task<IActionResult> DeleteLeague(int leagueId)
        {
            var result = await _leagueResetService.DeleteLeague(leagueId);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("start")]
        public async Task<IActionResult> StartLeague([FromBody] StartLeagueDto newLeague)
        {
            var result = await _leagueResetService.StartLeagueAsync(newLeague);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("getCurrentLeague")]
        public async Task<IActionResult> GetCurrentLeague()
        {
            var result = await _leagueResetService.GetCurrentLeague();
            return Ok(result);
        }

    }
}
