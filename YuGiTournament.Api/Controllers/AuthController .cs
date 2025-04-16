using Microsoft.AspNetCore.Mvc;
using YuGiTournament.Api.DTOs;
using YuGiTournament.Api.ApiResponses;
using YuGiTournament.Api.Services.Abstractions;

namespace YuGiTournament.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var (success, message, token) = await _authService.LoginAsync(model);
            if (!success)
            {
                return Unauthorized(new ApiResponse(false, message));
            }

            return Ok(new { Success = true, Message = message, Token = token });
        }

        [HttpPost("player-login")]
        public async Task<IActionResult> PlayerLogin([FromBody] PlayerLoginDto model)
        {
            var (success, message, token) = await _authService.PlayerLoginAsync(model);
            if (!success)
            {
                return Unauthorized(new ApiResponse(false, message));
            }

            return Ok(new { Success = true, Message = message, Token = token });
        }

        [HttpPost("register-player")]
        public async Task<IActionResult> RegisterPlayer([FromBody] RegisterPlayerDto model)
        {
            var (success, message) = await _authService.RegisterPlayerAsync(model);
            if (!success)
            {
                return BadRequest(new ApiResponse(false, message));
            }

            return Ok(new ApiResponse(true, message));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            var (success, message) = await _authService.ResetPasswordAsync(model);
            if (!success)
            {
                return BadRequest(new ApiResponse(false, message));
            }

            return Ok(new ApiResponse(true, message));
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new ApiResponse(true, "Logout successful"));
        }
    }


}