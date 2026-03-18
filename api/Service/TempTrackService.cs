using api.Dtos.TempTrack;
using api.Dtos.Track;
using api.Interfaces;
using api.Mappers;
using api.Models;
using System.Collections.Concurrent;

namespace api.Services;

public class TempTrackService : ITempTrackService
{
    private readonly ITrackRepository _trackRepository;
    private readonly ITempTokenService _tempTokenService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<TempTrackService> _logger;

    // In-memory store for temporary uploads (consider using distributed cache for production)
    private static readonly ConcurrentDictionary<string, TempUploadInfo> _tempUploads = new();

    private static readonly TimeSpan TempFileExpiration = TimeSpan.FromHours(2);

    public TempTrackService(
        ITrackRepository trackRepository,
        ITempTokenService tempTokenService,
        IWebHostEnvironment environment,
        ILogger<TempTrackService> logger)
    {
        _trackRepository = trackRepository;
        _tempTokenService = tempTokenService;
        _environment = environment;
        _logger = logger;
    }

    #region Temporary Track Operations

    public async Task<TempUploadResponse> UploadTempTrackAsync(IFormFile file, string? title)
    {
        var tempId = Guid.NewGuid().ToString();
        var tempAccessToken = _tempTokenService.GenerateToken();
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        // Create temp directory
        var tempPath = Path.Combine(_environment.ContentRootPath, "uploads", "temp");
        Directory.CreateDirectory(tempPath);

        var uniqueFileName = $"{tempId}{extension}";
        var filePath = Path.Combine(tempPath, uniqueFileName);

        // Save file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var trackTitle = string.IsNullOrWhiteSpace(title)
            ? Path.GetFileNameWithoutExtension(file.FileName)
            : title;

        var expiresAt = DateTime.UtcNow.Add(TempFileExpiration);
        var relativePath = $"/uploads/temp/{uniqueFileName}";

        var tempInfo = new TempUploadInfo
        {
            TempId = tempId,
            TempAccessTokenHash = _tempTokenService.HashToken(tempAccessToken),
            TempFilePath = filePath,
            RelativeFilePath = relativePath,
            OriginalFileName = file.FileName,
            Title = trackTitle,
            FileSize = file.Length,
            ContentType = file.ContentType,
            ExpiresAt = expiresAt
        };

        _tempUploads[tempId] = tempInfo;

        return new TempUploadResponse
        {
            TempId = tempId,
            TempAccessToken = tempAccessToken,
            TempFilePath = relativePath,
            OriginalFileName = file.FileName,
            Title = trackTitle,
            FileSize = file.Length,
            ContentType = file.ContentType,
            ExpiresAt = expiresAt
        };
    }

    public Task<TempUploadResponse?> GetTempTrackAsync(string tempId, string tempAccessToken)
    {
        if (!TryGetActiveTempUpload(tempId, tempAccessToken, out var tempInfo))
            return Task.FromResult<TempUploadResponse?>(null);

        return Task.FromResult<TempUploadResponse?>(new TempUploadResponse
        {
            TempId = tempInfo.TempId,
            TempAccessToken = tempAccessToken,
            TempFilePath = tempInfo.RelativeFilePath,
            OriginalFileName = tempInfo.OriginalFileName,
            Title = tempInfo.Title,
            FileSize = tempInfo.FileSize,
            ContentType = tempInfo.ContentType,
            ExpiresAt = tempInfo.ExpiresAt
        });
    }

    public async Task<TempUploadResponse?> UpdateTempTrackFileAsync(string tempId, IFormFile file, string tempAccessToken)
    {
        if (!TryGetActiveTempUpload(tempId, tempAccessToken, out var tempInfo))
            return null;

        // Delete old file
        if (File.Exists(tempInfo.TempFilePath))
        {
            File.Delete(tempInfo.TempFilePath);
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var tempPath = Path.Combine(_environment.ContentRootPath, "uploads", "temp");
        var uniqueFileName = $"{tempId}{extension}";
        var filePath = Path.Combine(tempPath, uniqueFileName);

        // Save new file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var relativePath = $"/uploads/temp/{uniqueFileName}";

        // Update temp info
        tempInfo.TempFilePath = filePath;
        tempInfo.RelativeFilePath = relativePath;
        tempInfo.OriginalFileName = file.FileName;
        tempInfo.FileSize = file.Length;
        tempInfo.ContentType = file.ContentType;
        tempInfo.ExpiresAt = DateTime.UtcNow.Add(TempFileExpiration);

        return new TempUploadResponse
        {
            TempId = tempInfo.TempId,
            TempAccessToken = tempAccessToken,
            TempFilePath = relativePath,
            OriginalFileName = tempInfo.OriginalFileName,
            Title = tempInfo.Title,
            FileSize = tempInfo.FileSize,
            ContentType = tempInfo.ContentType,
            ExpiresAt = tempInfo.ExpiresAt
        };
    }

    public Task<bool> DeleteTempTrackAsync(string tempId, string tempAccessToken)
    {
        if (!TryGetActiveTempUpload(tempId, tempAccessToken, out _))
            return Task.FromResult(false);

        return Task.FromResult(CleanupTempFile(tempId));
    }

    public async Task<Track> SaveTempTrackAsync(Guid userId, SaveTempTrackRequest request)
    {
        if (!_tempUploads.TryGetValue(request.TempId, out var tempInfo))
            throw new InvalidOperationException("Temporary track not found or expired.");

        if (DateTime.UtcNow > tempInfo.ExpiresAt)
        {
            CleanupTempFile(request.TempId);
            throw new InvalidOperationException("Temporary track has expired.");
        }

        if (!_tempTokenService.IsTokenValid(tempInfo.TempAccessTokenHash, request.TempAccessToken))
            throw new UnauthorizedAccessException("Invalid temporary track access token.");

        // Move file from temp to user's permanent location
        var extension = Path.GetExtension(tempInfo.TempFilePath);
        var userTracksPath = Path.Combine(_environment.ContentRootPath, "uploads", "tracks", userId.ToString());
        Directory.CreateDirectory(userTracksPath);

        var uniqueFileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid()}{extension}";
        var permanentFilePath = Path.Combine(userTracksPath, uniqueFileName);

        // Move the file
        File.Move(tempInfo.TempFilePath, permanentFilePath);

        var relativeFilePath = $"/uploads/tracks/{userId}/{uniqueFileName}";

        var createRequest = new CreateTrackRequest
        {
            Title = request.Title,
            FilePath = relativeFilePath
        };

        // Use repository directly to create permanent track
        var track = await _trackRepository.CreateTrackAsync(
            createRequest.ToTrackFromCreateTrackRequest(userId));

        // Clean up temp record
        _tempUploads.TryRemove(request.TempId, out _);

        return track;
    }

    #endregion

    #region Private Helpers

    private bool CleanupTempFile(string tempId)
    {
        if (_tempUploads.TryRemove(tempId, out var tempInfo))
        {
            try
            {
                if (File.Exists(tempInfo.TempFilePath))
                {
                    File.Delete(tempInfo.TempFilePath);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting temp file: {FilePath}", tempInfo.TempFilePath);
            }
        }
        return false;
    }

    private bool TryGetActiveTempUpload(string tempId, string tempAccessToken, out TempUploadInfo tempInfo)
    {
        tempInfo = null!;

        if (!_tempUploads.TryGetValue(tempId, out var storedTempInfo))
            return false;

        if (DateTime.UtcNow > storedTempInfo.ExpiresAt)
        {
            CleanupTempFile(tempId);
            return false;
        }

        if (!_tempTokenService.IsTokenValid(storedTempInfo.TempAccessTokenHash, tempAccessToken))
            return false;

        tempInfo = storedTempInfo;
        return true;
    }

    #endregion

    // Internal class to store temp upload info
    private class TempUploadInfo
    {
        public string TempId { get; set; } = string.Empty;
        public string TempAccessTokenHash { get; set; } = string.Empty;
        public string TempFilePath { get; set; } = string.Empty;
        public string RelativeFilePath { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
