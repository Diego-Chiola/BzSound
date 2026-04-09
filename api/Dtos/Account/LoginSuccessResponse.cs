namespace api.Dtos.Account;

public record LoginSuccessResponse(
    bool Success,
    string? AccessToken,
    string? RefreshToken,
    string? ErrorMessage = null)
{
    public static LoginSuccessResponse SuccessResponse(string accessToken, string refreshToken)
        => new(true, accessToken, refreshToken);
    public static LoginSuccessResponse FailureResponse(string errorMessage)
        => new(false, null, null, errorMessage);
}