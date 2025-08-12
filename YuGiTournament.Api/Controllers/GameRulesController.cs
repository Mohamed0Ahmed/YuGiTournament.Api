using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YuGiTournament.Api.Services.Abstractions;

namespace YuGiTournament.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameRulesController : ControllerBase
    {
        private readonly IGameRulesService _gameRulesService;

        public GameRulesController(IGameRulesService gameRulesService)
        {
            _gameRulesService = gameRulesService;
        }

        [HttpGet("max-rounds")]
        public async Task<IActionResult> GetMaxRounds()
        {
            var value = await _gameRulesService.GetMaxRoundsPerMatchAsync();
            return Ok(new { success = true, maxRoundsPerMatch = value });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("max-rounds")]
        public async Task<IActionResult> SetMaxRounds([FromBody] SetMaxRoundsDto dto)
        {
            if (dto == null || dto.MaxRoundsPerMatch <= 0)
                return BadRequest(new { success = false, message = "قيمة غير صالحة" });

            var cfg = await _gameRulesService.UpsertMaxRoundsPerMatchAsync(dto.MaxRoundsPerMatch);
            return Ok(new { success = true, config = cfg });
        }
    }

    public class SetMaxRoundsDto
    {
        public int MaxRoundsPerMatch { get; set; }
    }
}


