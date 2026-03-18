using api.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace api.Services;

public class TempTokenService : ITempTokenService
{
    public string GenerateToken()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    }

    public string HashToken(string token)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hashBytes);
    }

    public bool IsTokenValid(string storedTokenHash, string providedToken)
    {
        if (string.IsNullOrWhiteSpace(storedTokenHash) || string.IsNullOrWhiteSpace(providedToken))
            return false;

        var providedTokenHash = HashToken(providedToken);
        return string.Equals(storedTokenHash, providedTokenHash, StringComparison.Ordinal);
    }
}
