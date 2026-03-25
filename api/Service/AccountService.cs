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

    public AccountService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        ITokenService tokenService,
        IEmailService emailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _emailService = emailService;
    }

    public async Task<(bool Success, AuthenticationSuccessResponse? Data, string? Error)> RegisterAsync(RegisterRequest request)
    {
        var appUser = request.ToAppUser();

        var createdUser = await _userManager.CreateAsync(appUser, request.Password!);
        if (!createdUser.Succeeded)
        {
            return (false, null, string.Join(", ", createdUser.Errors.Select(e => e.Description)));
        }

        var roleResult = await _userManager.AddToRoleAsync(appUser, "User");
        if (!roleResult.Succeeded)
        {
            return (false, null, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }

        var response = appUser.ToAuthenticationSuccessResponse(_tokenService.CreateToken(appUser));
        return (true, response, null);
    }

    public async Task<(bool Success, AuthenticationSuccessResponse? Data, string? Error)> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email!);
        if (user == null)
        {
            return (false, null, "Invalid email or password.");
        }

        var passwordValid = await _signInManager.CheckPasswordSignInAsync(user, request.Password!, false);
        if (!passwordValid.Succeeded)
        {
            return (false, null, "Invalid email or password.");
        }

        var response = new AuthenticationSuccessResponse
        {
            Email = user.Email,
            Token = _tokenService.CreateToken(user)
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

}