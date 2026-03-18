namespace api.Interfaces;

public interface ITempTokenService
{
    string GenerateToken();
    string HashToken(string token);
    bool IsTokenValid(string storedTokenHash, string providedToken);
}
