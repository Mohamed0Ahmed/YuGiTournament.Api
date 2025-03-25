using Microsoft.AspNetCore.Mvc;
using YuGiTournament.Api.Models;
using YuGiTournament.Api.Services.Abstractions;

namespace YuGiTournament.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerController : ControllerBase
    {
        private readonly IPlayerService _playerService;

        public PlayerController(IPlayerService playerService)
        {
            _playerService = playerService;
        }

        [HttpPost]
        public async Task<IActionResult> AddPlayer([FromBody] string fullName)
        {
            await _playerService.AddPlayerAsync(fullName);
            var Player = new Player
            {
                FullName = fullName,
            };
            return Ok(Player);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPlayers()
        {
            var players = await _playerService.GetAllPlayersAsync();
            return Ok(players);
        }

        [HttpDelete("{playerId}")]
        public async Task<IActionResult> DeletePlayer(int playerId)
        {
            var result = await _playerService.DeletePlayerAsync(playerId);
            if (!result)
                return NotFound("Player not found or already deleted.");

            return NoContent();
        }
    }
}
