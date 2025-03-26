using Microsoft.AspNetCore.Authorization;
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
        [Authorize]
        public async Task<IActionResult> AddPlayer([FromBody] string fullName)
        {
            await _playerService.AddPlayerAsync(fullName);
            var player = new Player
            {
                FullName = fullName,
            };
            return Ok(player);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPlayers()
        {
            var players = await _playerService.GetAllPlayersAsync();
            return Ok(players);
        }

        [Authorize]
        [HttpDelete("{playerId}")]
        public async Task<IActionResult> DeletePlayer(int playerId)
        {
            Console.WriteLine($"DeletePlayer called with playerId: {playerId}");
            var result = await _playerService.DeletePlayerAsync(playerId);
            return Ok(result);
        }
    }
}