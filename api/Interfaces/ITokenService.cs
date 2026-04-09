using api.Models;
using System.Security.Claims;

namespace api.Interfaces;

public interface ITokenService
{
    string CreateAccessToken(AppUser user);
    string CreateRefreshToken(AppUser user);
    ClaimsPrincipal? ValidateRefreshToken(string refreshToken);
}