using api.Dtos.Account;
using api.Models;

namespace api.Mappers;

public static class AccountMapper
{
    public static AppUser ToAppUser(this RegisterRequest registerRequest)
    {
        return new AppUser
        {
            Email = registerRequest.Email,
            UserName = registerRequest.Email
        };
    }

    public static LoginSuccessResponse ToLoginSuccessResponse(this AppUser appUser, string token)
    {
        return new LoginSuccessResponse
        {
            Email = appUser.Email,
            Token = token
        };
    }
}