using Shared.Models.DTOs;
using Shared.Models.Entities;
namespace Shared.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<bool> ConfirmEmailAsync(string email, string token);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
        string GenerateJwtToken(User user);
    }
}
