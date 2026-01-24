using api.Dtos.Account;
using api.Interfaces;
using api.Models;

public static class AccountMapper
{
    public static AppUser ToAppUser(this RegisterRequest registerRequest)
    {
        return new AppUser
        {
            UserName = registerRequest.Username,
            Email = registerRequest.Email
        };
    }

    public static AuthenticationSuccessResponse ToAuthenticationSuccessResponse(this AppUser appUser, string token)
    {
        return new AuthenticationSuccessResponse
        {
            Email = appUser.Email,
            Token = token
        };
    }
}