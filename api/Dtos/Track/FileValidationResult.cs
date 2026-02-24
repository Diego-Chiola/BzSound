namespace api.Dtos.Track;

public class FileValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }

    public static FileValidationResult Success() => new() { IsValid = true };
    public static FileValidationResult Failure(string message) => new() { IsValid = false, ErrorMessage = message };
}
