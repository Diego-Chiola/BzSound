using api.Dtos.Account;

namespace api.Interfaces;

public interface IAccountService
{
    Task<RegisterSuccessResponse> RegisterAsync(RegisterRequest request);
    Task<LoginSuccessResponse> LoginAsync(LoginRequest request);
    Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken);
    Task<PasswordResetResponse> RequestPasswordResetAsync(string email);
    Task<ConfirmEmailResponse> ConfirmEmailAsync(string email, string token);
}