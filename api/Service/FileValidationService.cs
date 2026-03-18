using api.Dtos.Track;
using api.Interfaces;

namespace api.Service;

public class FileValidationService : IFileValidationService
{
    private const long MaxFileSize = 50 * 1024 * 1024; // 50MB
    private static readonly string[] AllowedExtensions = [".mp3", ".wav", ".ogg", ".m4a", ".flac"];
    public FileValidationResult ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return FileValidationResult.Failure("No file uploaded.");

        if (file.Length > MaxFileSize)
            return FileValidationResult.Failure($"File size exceeds maximum allowed size of {MaxFileSize / (1024 * 1024)}MB.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return FileValidationResult.Failure($"Invalid file type. Allowed types: {string.Join(", ", AllowedExtensions)}");

        return FileValidationResult.Success();
    }
}