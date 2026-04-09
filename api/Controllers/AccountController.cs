using api.Dtos.Account;
using api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("api/auth")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        var (success, error) = await _accountService.RegisterAsync(registerRequest);

        if (!success)
            return StatusCode(500, new { message = error });

        return Ok(new { message = "User registered successfully." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        var (success, accessToken, refreshToken, error) = await _accountService.LoginAsync(loginRequest);

        if (!success)
            return Unauthorized(new { message = error });

        return Ok(new { AccessToken = accessToken, RefreshToken = refreshToken });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var (success, accessToken, error) = await _accountService.RefreshTokenAsync(request.RefreshToken!);

        if (!success)
            return Unauthorized(new { message = error });

        return Ok(new { AccessToken = accessToken });
    }

    [HttpGet("password-reset")]
    public async Task<IActionResult> RequestPasswordReset([FromQuery] string email)
    {
        var (success, error) = await _accountService.RequestPasswordResetAsync(email);

        if (!success)
            return BadRequest(new { message = error });

        return Ok(new { message = "Password reset link sent if email exists." });
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
    {
        var (success, error) = await _accountService.ConfirmEmailAsync(email, token);

        if (!success)
            return BadRequest(new { message = error });

        return Ok(new { message = "Email confirmed successfully." });
    }
}