using api.Dtos.Account;
using api.Models;

namespace api.Mappers;

public static class AccountMapper
{
    public static AppUser ToAppUser(this RegisterRequest registerRequest)
    {
        return new AppUser
        {
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