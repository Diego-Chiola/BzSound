namespace api.Dtos.Account;

public record RefreshTokenResponse(
    bool Success,
    string? AccessToken,
    string? ErrorMessage = null)
{
    public static RefreshTokenResponse SuccessResponse(string accessToken)
        => new(true, accessToken);
    public static RefreshTokenResponse FailureResponse(string errorMessage)
        => new(false, null, errorMessage);
}