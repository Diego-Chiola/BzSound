using System.Text;
using api.Dtos.Track;
using api.Interfaces;

namespace api.Service;

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    private const long MaxFileSize = 50 * 1024 * 1024; // 50MB
    private static readonly string[] AllowedExtensions = [".mp3", ".wav", ".ogg", ".m4a", ".flac"];

    public FileService(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    public FileValidationResult ValidateAudioFile(IFormFile file)
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

    public string SanitizeFileName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "file";

        var invalidChars = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(value.Length);

        foreach (var character in value.Trim())
        {
            builder.Append(Array.IndexOf(invalidChars, character) >= 0 ? '_' : character);
        }

        var result = builder.ToString().Trim(' ', '.');
        return string.IsNullOrWhiteSpace(result) ? "file" : result;
    }

    public async Task<string> SaveFileAsync(IFormFile file, params string[] relativePathSegments)
    {
        if (relativePathSegments == null || relativePathSegments.Length == 0)
            throw new ArgumentException("At least one path segment is required.", nameof(relativePathSegments));

        var safeSegments = relativePathSegments
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(SanitizeFileName)
            .ToArray();

        if (safeSegments.Length == 0)
            throw new ArgumentException("At least one non-empty path segment is required.", nameof(relativePathSegments));

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        safeSegments[^1] = $"{safeSegments[^1]}{extension}";

        var directorySegments = safeSegments.Take(safeSegments.Length - 1).ToArray();
        var fileName = safeSegments[^1];

        var directoryPath = Path.Combine(_webHostEnvironment.ContentRootPath, "uploads");
        if (directorySegments.Length > 0)
            directoryPath = Path.Combine(directoryPath, Path.Combine(directorySegments));

        Directory.CreateDirectory(directoryPath);

        var absolutePath = Path.Combine(directoryPath, fileName);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var suffix = 1;

        while (File.Exists(absolutePath))
        {
            var suffixedName = $"{fileNameWithoutExtension}_{suffix}{extension}";
            absolutePath = Path.Combine(directoryPath, suffixedName);
            fileName = suffixedName;
            suffix++;
        }

        await using var stream = new FileStream(absolutePath, FileMode.CreateNew);
        await file.CopyToAsync(stream);

        var relativeParts = directorySegments.Append(fileName).ToArray();
        return "/uploads/" + string.Join("/", relativeParts);
    }

    public Task<bool> DeleteFileAsync(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return Task.FromResult(false);

        var normalizedPath = relativePath.Replace('\\', '/').Trim();
        if (!normalizedPath.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(false);

        var relativeWithoutPrefix = normalizedPath["/uploads/".Length..];
        if (string.IsNullOrWhiteSpace(relativeWithoutPrefix))
            return Task.FromResult(false);

        var safeSegments = relativeWithoutPrefix
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(SanitizeFileName)
            .ToArray();

        if (safeSegments.Length == 0)
            return Task.FromResult(false);

        var absolutePath = Path.Combine(_webHostEnvironment.ContentRootPath, "uploads", Path.Combine(safeSegments));

        if (!File.Exists(absolutePath))
            return Task.FromResult(false);

        File.Delete(absolutePath);
        return Task.FromResult(true);
    }

    public async Task<int?> GetAudioDurationSecondsAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return null;

        await using var stream = file.OpenReadStream();

        // TagLib needs a stream abstraction
        var abstraction = new StreamFileAbstraction(file.FileName, stream, stream);
        using var tagFile = TagLib.File.Create(abstraction);

        return (int)Math.Round(tagFile.Properties.Duration.TotalSeconds);
    }

    private sealed class StreamFileAbstraction(string name, Stream readStream, Stream writeStream) : TagLib.File.IFileAbstraction
    {
        public string Name { get; } = name;
        public Stream ReadStream { get; } = readStream;
        public Stream WriteStream { get; } = writeStream;
        public void CloseStream(Stream stream) { /* stream disposed by caller */ }
    }
}
