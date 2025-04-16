using YuGiTournament.Api.DTOs;
using YuGiTournament.Api.Identities;

namespace YuGiTournament.Api.Services.Abstractions
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, string Token)> LoginAsync(LoginDto model);
        Task<(bool Success, string Message, string Token)> PlayerLoginAsync(PlayerLoginDto model);
        Task<(bool Success, string Message)> RegisterPlayerAsync(RegisterPlayerDto model);
        Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordDto model);
        string GenerateJwtToken(ApplicationUser user, IList<string> roles);
    }
}
