using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using YuGiTournament.Api.DTOs;
using YuGiTournament.Api.Identities;
using YuGiTournament.Api.Services.Abstractions;

namespace YuGiTournament.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<(bool Success, string Message, string Token)> LoginAsync(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await _userManager.CheckPasswordAsync(user, model.Password)))
            {
                return (false, "Invalid email or password.", null)!;
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);
            return (true, "Login successful.", token);
        }

        public async Task<(bool Success, string Message, string Token)> PlayerLoginAsync(PlayerLoginDto model)
        {
            var user = await _userManager.FindByNameAsync(model.PhoneNumber);
            if (user == null || !(await _userManager.CheckPasswordAsync(user, model.Password)))
            {
                return (false, "Invalid phone number or password.", null)!;
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);
            return (true, "Login successful.", token);
        }

        public async Task<(bool Success, string Message)> RegisterPlayerAsync(RegisterPlayerDto model)
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
                return (false, errorMessage);
            }

            await _userManager.AddToRoleAsync(user, "Player");
            return (true, "Player registered successfully.");
        }

        public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordDto model)
        {
            var user = await _userManager.FindByNameAsync(model.PhoneNumber);
            if (user == null || user.UserName == "admin@yugi.com")
            {
                return (false, "Player not found.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
            if (!result.Succeeded)
            {
                var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, errorMessage);
            }

            return (true, "Password reset successfully.");
        }

        public string GenerateJwtToken(ApplicationUser user, IList<string> roles)
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
