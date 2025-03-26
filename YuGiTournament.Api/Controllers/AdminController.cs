using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YuGiTournament.Api.Services.Abstractions;
using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        //private readonly IPlayerService _playerService;
        //private readonly IMatchService _matchService;

        //public AdminController(IPlayerService playerService, IMatchService matchService)
        //{
        //    _playerService = playerService;
        //    _matchService = matchService;
        //}

        //[HttpPost("add-player")]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> AddPlayer([FromBody] Player player)
        //{
        //    var result = await _playerService.AddPlayerAsync(player);
        //    if (!result) return BadRequest("Failed to add player");
        //    return Ok("Player added successfully");
        //}

        //[HttpPost("set-match-result")]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> SetMatchResult([FromBody] MatchResultModel matchResult)
        //{
        //    var result = await _matchService.UpdateMatchResultAsync(matchResult);
        //    if (!result) return BadRequest("Failed to set match result");
        //    return Ok("Match result updated successfully");
        //}
    }
}
