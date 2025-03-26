using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        [HttpPost("reset")]
        public async Task<IActionResult> ResetLeague()
        {
            await _leagueResetService.ResetLeagueAsync();
            return Ok("League has been reset successfully.");
        }
    }
}
