namespace api.Dtos.Account;

public record RegisterSuccessResponse(
    bool Success,
    string? ErrorMessage = null)
{
    public static RegisterSuccessResponse SuccessResponse()
        => new(true);
    public static RegisterSuccessResponse FailureResponse(string errorMessage)
        => new(false, errorMessage);
}
