using api.Data;
using api.Dtos.Account;
using api.Interfaces;
using api.Mappers;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace api.Service;

public class AccountService : IAccountService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly ApplicationDBContext _dbContext;

    public AccountService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        ITokenService tokenService,
        IEmailService emailService,
        ApplicationDBContext dbContext)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _emailService = emailService;
        _dbContext = dbContext;
    }

    public async Task<RegisterSuccessResponse> RegisterAsync(RegisterRequest request)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var appUser = request.ToAppUser();

            var createdUser = await _userManager.CreateAsync(appUser, request.Password!);
            if (!createdUser.Succeeded)
            {
                await transaction.RollbackAsync();
                return RegisterSuccessResponse.FailureResponse(string.Join(", ", createdUser.Errors.Select(e => e.Description)));
            }

            var roleResult = await _userManager.AddToRoleAsync(appUser, "User");
            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return RegisterSuccessResponse.FailureResponse(string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }

            var emailCode = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
            var validEmailCode = WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(emailCode));

            if (!await _emailService.SendConfirmationEmailAsync(appUser.Email!, validEmailCode))
            {
                await transaction.RollbackAsync();
                return RegisterSuccessResponse.FailureResponse("Failed to send email confirmation. Please try again later.");
            }

            await transaction.CommitAsync();

            return RegisterSuccessResponse.SuccessResponse();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return RegisterSuccessResponse.FailureResponse($"An error occurred during registration: {ex.Message}");
        }
    }

    public async Task<LoginSuccessResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email!);
        if (user == null)
        {
            return LoginSuccessResponse.FailureResponse("Invalid email or password.");
        }

        if (!user.EmailConfirmed)
        {
            return LoginSuccessResponse.FailureResponse("Please confirm your email before logging in.");
        }

        var passwordValid = await _signInManager.CheckPasswordSignInAsync(user, request.Password!, false);
        if (!passwordValid.Succeeded)
        {
            return LoginSuccessResponse.FailureResponse("Invalid email or password.");
        }

        var accessToken = _tokenService.CreateAccessToken(user);
        var refreshToken = _tokenService.CreateRefreshToken(user);

        return LoginSuccessResponse.SuccessResponse(accessToken, refreshToken);
    }

    public async Task<PasswordResetResponse> RequestPasswordResetAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return PasswordResetResponse.FailureResponse("Invalid Email.");
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        string validToken = WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(resetToken));

        var isResetEmailSent = await _emailService.SendPasswordResetEmailAsync(email, validToken);
        if (!isResetEmailSent)
        {
            return PasswordResetResponse.FailureResponse("Unable to send password reset email at this time.");
        }

        return PasswordResetResponse.SuccessResponse();
    }

    public async Task<ConfirmEmailResponse> ConfirmEmailAsync(string email, string token)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return ConfirmEmailResponse.FailureResponse("Invalid email.");
        }

        if (user.EmailConfirmed)
        {
            return ConfirmEmailResponse.SuccessResponse();
        }

        string decodedToken;
        try
        {
            var decodedBytes = WebEncoders.Base64UrlDecode(token);
            decodedToken = System.Text.Encoding.UTF8.GetString(decodedBytes);
        }
        catch (FormatException)
        {
            return ConfirmEmailResponse.FailureResponse("Invalid confirmation token.");
        }

        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
        if (!result.Succeeded)
        {
            return ConfirmEmailResponse.FailureResponse(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return ConfirmEmailResponse.SuccessResponse();
    }

    public async Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return RefreshTokenResponse.FailureResponse("Refresh token is required.");
        }

        var principal = _tokenService.ValidateRefreshToken(refreshToken);
        if (principal == null)
        {
            return RefreshTokenResponse.FailureResponse("Invalid or expired refresh token.");
        }

        var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return RefreshTokenResponse.FailureResponse("Invalid token claims.");
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return RefreshTokenResponse.FailureResponse("User not found.");
        }

        var accessToken = _tokenService.CreateAccessToken(user);

        return RefreshTokenResponse.SuccessResponse(accessToken);
    }

}