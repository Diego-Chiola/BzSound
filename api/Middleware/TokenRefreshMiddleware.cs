using System.IdentityModel.Tokens.Jwt;
using api.Interfaces;

namespace api.Middleware;

public class TokenRefreshMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenRefreshMiddleware> _logger;

    public TokenRefreshMiddleware(RequestDelegate next, ILogger<TokenRefreshMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAccountService accountService)
    {
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();

            // Check if access token is expired
            if (IsTokenExpired(token))
            {
                _logger.LogInformation("Access token expired, attempting to refresh...");

                var refreshToken = context.Request.Headers["X-Refresh-Token"].FirstOrDefault();

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    // Try to refresh the token
                    var response = await accountService.RefreshTokenAsync(refreshToken);

                    if (response.Success && !string.IsNullOrEmpty(response.AccessToken))
                    {
                        // Update the authorization header with the new token
                        context.Request.Headers.Authorization = $"Bearer {response.AccessToken}";

                        // Set response header with new token so client can update it
                        context.Response.Headers.Append("X-New-Access-Token", response.AccessToken);

                        _logger.LogInformation("Token refreshed successfully");
                    }
                    else
                    {
                        _logger.LogWarning("Failed to refresh token: {Error}", response.ErrorMessage);
                    }
                }
                else
                {
                    _logger.LogWarning("Expired access token but no refresh token provided");
                }
            }
        }

        await _next(context);
    }

    private static bool IsTokenExpired(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        return jwtToken.ValidTo < DateTime.UtcNow;
    }
}
