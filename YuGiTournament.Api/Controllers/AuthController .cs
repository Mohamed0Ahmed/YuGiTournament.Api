using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using YuGiTournament.Api.Identities;
using YuGiTournament.Api.DTOs;
using YuGiTournament.Api.ApiResponses;

namespace YuGiTournament.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IConfiguration _configuration;

        public AuthController(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await _userManager.CheckPasswordAsync(user, model.Password)))
            {
                return Unauthorized(new ApiResponse(false, "Invalid email or password."));
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);
            return Ok(new { Success = true, Message = "Login successful.", Token = token });
        }

        [HttpPost("player-login")]
        public async Task<IActionResult> PlayerLogin([FromBody] PlayerLoginDto model)
        {
            var user = await _userManager.FindByNameAsync(model.PhoneNumber);
            if (user == null || !(await _userManager.CheckPasswordAsync(user, model.Password)))
            {
                return Unauthorized(new ApiResponse(false, "Invalid phone number or password."));
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);
            return Ok(new { Success = true, Message = "Login successful.", Token = token });
        }

        [HttpPost("register-player")]
        public async Task<IActionResult> RegisterPlayer([FromBody] RegisterPlayerDto model)
        {
            var user = new ApplicationUser
            {
                UserName = model.PhoneNumber,
                PhoneNumber = model.PhoneNumber,
                Email = $"{model.PhoneNumber}@yugi.com",
                EmailConfirmed = true,
                FName = model.FirstName,
                LName = model.LastName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new ApiResponse(false, errorMessage));
            }

            await _userManager.AddToRoleAsync(user, "Player");
            return Ok(new ApiResponse(true, "Player registered successfully."));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            var user = await _userManager.FindByNameAsync(model.PhoneNumber);
            if (user == null)
            {
                return NotFound(new ApiResponse(false, "Player not found."));
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
            if (!result.Succeeded)
            {
                var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new ApiResponse(false, errorMessage));
            }

            return Ok(new ApiResponse(true, "Password reset successfully."));
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new ApiResponse(true, "Logout successful. Just remove the token on client side."));
        }

        //****************************************

        private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new (JwtRegisteredClaimNames.Sub, user.Id),
                new (JwtRegisteredClaimNames.Email, user.Email!),
                new (ClaimTypes.Name, user.UserName!)
            };

            claims.AddRange(roles.Select(role => new Claim("role", role)));

            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new InvalidOperationException("JWT Key is missing from configuration.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expireDays = int.Parse(_configuration["Jwt:ExpireDays"] ?? "7");

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(expireDays),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

   
}