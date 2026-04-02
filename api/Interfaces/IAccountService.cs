using api.Dtos.Account;

namespace api.Interfaces;

public interface IAccountService
{
    Task<(bool Success, RegisterSuccessResponse? Data, string? Error)> RegisterAsync(RegisterRequest request);
    Task<(bool Success, LoginSuccessResponse? Data, string? Error)> LoginAsync(LoginRequest request);
    Task<PasswordResetResponse> RequestPasswordResetAsync(string email);
    Task<(bool Success, string? Error)> ConfirmEmailAsync(string email, string token);
}