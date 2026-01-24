using api.Dtos.Account;

namespace api.Interfaces;

public interface IAccountService
{
    Task<(bool Success, AuthenticationSuccessResponse? Data, string? Error)> RegisterAsync(RegisterRequest request);
    Task<(bool Success, AuthenticationSuccessResponse? Data, string? Error)> LoginAsync(LoginRequest request);
}