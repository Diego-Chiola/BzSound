namespace api.Dtos.Account;

public record PasswordResetResponse(
    bool Success,
    string? ErrorMessage = null)
{
    public static PasswordResetResponse SuccessResponse()
        => new(true);

    public static PasswordResetResponse FailureResponse(string errorMessage)
        => new(false, errorMessage);
}