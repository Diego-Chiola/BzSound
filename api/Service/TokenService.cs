using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace api.Service;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly SymmetricSecurityKey _key;
    private readonly UserManager<AppUser> _userManager;

    public TokenService(IConfiguration configuration, UserManager<AppUser> userManager)
    {
        _config = configuration;
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:SigningKey"]!));
        _userManager = userManager;
    }

    public string CreateRefreshToken(AppUser user)
    {
        return CreateToken(user, isRefreshToken: true);
    }

    public string CreateAccessToken(AppUser user)
    {
        return CreateToken(user, isRefreshToken: false);
    }

    public ClaimsPrincipal? ValidateRefreshToken(string refreshToken)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _key,
            ValidateIssuer = true,
            ValidIssuer = _config["JWT:Issuer"],
            ValidateAudience = true,
            ValidAudience = _config["JWT:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(refreshToken, tokenValidationParameters, out SecurityToken validatedToken);

            var tokenType = principal.FindFirst("type")?.Value;
            if (tokenType != "refresh")
                return null;

            return principal;
        }
        catch
        {
            return null;
        }
    }

    private string CreateToken(AppUser user, bool isRefreshToken = false)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        if (isRefreshToken)
        {
            claims.Add(new Claim("type", "refresh"));
        }

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = isRefreshToken ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = creds,
            Issuer = _config["JWT:Issuer"],
            Audience = _config["JWT:Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}