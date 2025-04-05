using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YuGiTournament.Api.DTOs;
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddPlayer([FromBody] PlayerAddedDto player)
        {
            var result = await _playerService.AddPlayerAsync(player.FullName);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPlayers()
        {
            var players = await _playerService.GetAllPlayersAsync();
            return Ok(players);
        }

        [HttpGet("ranking")]
        public async Task<IActionResult> GetRanking()
        {
            var ranking = await _playerService.GetPlayersRankingAsync();
            return Ok(ranking);
        }

        [HttpGet("ranking/all")]
        public async Task<IActionResult> GetRankingAll()
        {
            var ranking = await _playerService.GetAllLeaguesWithMatchesAsync();
            return Ok(ranking);
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("{playerId}")]
        public async Task<IActionResult> DeletePlayer(int playerId)
        {
            var result = await _playerService.DeletePlayerAsync(playerId);
            return Ok(result);
        }
    }
}