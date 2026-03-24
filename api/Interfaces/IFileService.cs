using api.Dtos.Track;

namespace api.Interfaces;

public interface IFileService
{
    FileValidationResult ValidateAudioFile(IFormFile file);
    string SanitizeFileName(string value);
    Task<string> SaveFileAsync(IFormFile file, params string[] relativePathSegments);
    Task<bool> DeleteFileAsync(string relativePath);
    Task<int?> GetAudioDurationSecondsAsync(IFormFile file);
}
