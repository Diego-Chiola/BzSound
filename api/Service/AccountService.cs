using api.Dtos.Account;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Identity;

namespace api.Service;

public class AccountService : IAccountService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ITokenService _tokenService;

    public AccountService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
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
}