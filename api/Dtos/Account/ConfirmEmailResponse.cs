namespace api.Dtos.Account;

public record ConfirmEmailResponse(
    bool Success,
    string? ErrorMessage = null)
{
    public static ConfirmEmailResponse SuccessResponse()
        => new(true);

    public static ConfirmEmailResponse FailureResponse(string errorMessage)
        => new(false, errorMessage);
}