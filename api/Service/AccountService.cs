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

    public async Task<(bool Success, AuthenticationSuccessResponse? Data, string? Error)> RegisterAsync(RegisterRequest request)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var appUser = request.ToAppUser();

            var createdUser = await _userManager.CreateAsync(appUser, request.Password!);
            if (!createdUser.Succeeded)
            {
                await transaction.RollbackAsync();
                return (false, null, string.Join(", ", createdUser.Errors.Select(e => e.Description)));
            }

            var roleResult = await _userManager.AddToRoleAsync(appUser, "User");
            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return (false, null, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }

            var emailCode = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
            var validEmailCode = WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(emailCode));

            if (!await _emailService.SendEmailConfirmationEmailAsync(appUser.Email!, validEmailCode))
            {
                await transaction.RollbackAsync();
                return (false, null, "Failed to send email confirmation. Please try again later.");
            }

            await transaction.CommitAsync();

            var token = await _tokenService.CreateToken(appUser);
            var response = appUser.ToAuthenticationSuccessResponse(token);
            return (true, response, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, null, $"An error occurred during registration: {ex.Message}");
        }
    }

    public async Task<(bool Success, AuthenticationSuccessResponse? Data, string? Error)> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email!);
        if (user == null)
        {
            return (false, null, "Invalid email or password.");
        }

        if (!user.EmailConfirmed)
        {
            return (false, null, "Please confirm your email before logging in.");
        }

        var passwordValid = await _signInManager.CheckPasswordSignInAsync(user, request.Password!, false);
        if (!passwordValid.Succeeded)
        {
            return (false, null, "Invalid email or password.");
        }

        var token = await _tokenService.CreateToken(user);
        var response = new AuthenticationSuccessResponse
        {
            Email = user.Email,
            Token = token
        };

        return (true, response, null);
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

    public async Task<(bool Success, string? Error)> ConfirmEmailAsync(string email, string token)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return (false, "Invalid email.");
        }

        if (user.EmailConfirmed)
        {
            return (true, null);
        }

        string decodedToken;
        try
        {
            var decodedBytes = WebEncoders.Base64UrlDecode(token);
            decodedToken = System.Text.Encoding.UTF8.GetString(decodedBytes);
        }
        catch (FormatException)
        {
            return (false, "Invalid confirmation token.");
        }

        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
        if (!result.Succeeded)
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return (true, null);
    }

}