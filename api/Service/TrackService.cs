using api.Dtos.Track;
using api.Helpers;
using api.Interfaces;
using api.Mappers;
using api.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Concurrent;

namespace api.Services;

public class TrackService : ITrackService
{
    private readonly ITrackRepository _trackRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<TrackService> _logger;

    // In-memory store for temporary uploads (consider using distributed cache for production)
    private static readonly ConcurrentDictionary<string, TempUploadInfo> _tempUploads = new();

    private const long MaxFileSize = 50 * 1024 * 1024; // 50MB
    private static readonly string[] AllowedExtensions = { ".mp3", ".wav", ".ogg", ".m4a", ".flac" };
    private static readonly TimeSpan TempFileExpiration = TimeSpan.FromHours(24);

    public TrackService(
        ITrackRepository trackRepository,
        UserManager<AppUser> userManager,
        IWebHostEnvironment environment,
        ILogger<TrackService> logger)
    {
        _trackRepository = trackRepository;
        _userManager = userManager;
        _environment = environment;
        _logger = logger;
    }

    #region File Validation

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

    #endregion

    #region Temporary Track Operations

    public async Task<TempUploadResponse> UploadTempTrackAsync(IFormFile file, string? title)
    {
        var tempId = Guid.NewGuid().ToString();
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
            TempFilePath = relativePath,
            OriginalFileName = file.FileName,
            Title = trackTitle,
            FileSize = file.Length,
            ContentType = file.ContentType,
            ExpiresAt = expiresAt
        };
    }

    public Task<TempUploadResponse?> GetTempTrackAsync(string tempId)
    {
        if (!_tempUploads.TryGetValue(tempId, out var tempInfo))
            return Task.FromResult<TempUploadResponse?>(null);

        if (DateTime.UtcNow > tempInfo.ExpiresAt)
        {
            // Clean up expired temp file
            CleanupTempFile(tempId);
            return Task.FromResult<TempUploadResponse?>(null);
        }

        return Task.FromResult<TempUploadResponse?>(new TempUploadResponse
        {
            TempId = tempInfo.TempId,
            TempFilePath = tempInfo.RelativeFilePath,
            OriginalFileName = tempInfo.OriginalFileName,
            Title = tempInfo.Title,
            FileSize = tempInfo.FileSize,
            ContentType = tempInfo.ContentType,
            ExpiresAt = tempInfo.ExpiresAt
        });
    }

    public async Task<TempUploadResponse?> UpdateTempTrackFileAsync(string tempId, IFormFile file)
    {
        if (!_tempUploads.TryGetValue(tempId, out var tempInfo))
            return null;

        if (DateTime.UtcNow > tempInfo.ExpiresAt)
        {
            CleanupTempFile(tempId);
            return null;
        }

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
            TempFilePath = relativePath,
            OriginalFileName = tempInfo.OriginalFileName,
            Title = tempInfo.Title,
            FileSize = tempInfo.FileSize,
            ContentType = tempInfo.ContentType,
            ExpiresAt = tempInfo.ExpiresAt
        };
    }

    public Task<bool> DeleteTempTrackAsync(string tempId)
    {
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

        var track = await CreateTrackAsync(userId, createRequest);

        // Clean up temp record
        _tempUploads.TryRemove(request.TempId, out _);

        return track;
    }

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

    #endregion

    #region Permanent Track Operations

    public async Task<bool> UserExistsAsync(Guid userId)
    {
        return await _userManager.FindByIdAsync(userId.ToString()) is not null;
    }

    public async Task<IEnumerable<GetTrackRequest>> GetTracksByUserAsync(Guid userId, TrackQueryObject query)
    {
        var tracks = await _trackRepository.GetTracksByUserAsync(userId, query);
        return tracks.Select(t => t.ToGetTrackRequestFromTrack());
    }

    public async Task<GetTrackRequest?> GetTrackAsync(Guid userId, int trackId)
    {
        var track = await _trackRepository.GetTrackAsync(trackId);

        if (track is null || track.UserId != userId)
            return null;

        return track.ToGetTrackRequestFromTrack();
    }

    public async Task<Track> CreateTrackAsync(Guid userId, CreateTrackRequest request)
    {
        var trackModel = await _trackRepository.CreateTrackAsync(
            request.ToTrackFromCreateTrackRequest(userId));

        return trackModel;
    }

    public async Task<UpdateTrackRequest?> UpdateTrackAsync(Guid userId, int trackId, UpdateTrackRequest request)
    {
        var existingTrack = await _trackRepository.GetTrackAsync(trackId);

        if (existingTrack is null || existingTrack.UserId != userId)
            return null;

        var updatedTrack = await _trackRepository.UpdateTrackAsync(userId, trackId, request);
        return updatedTrack?.ToUpdateTrackRequestFromTrack();
    }

    public async Task<bool> DeleteTrackAsync(Guid userId, int trackId)
    {
        var track = await _trackRepository.DeleteTrackAsync(userId, trackId);
        return track is not null;
    }

    #endregion

    // Internal class to store temp upload info
    private class TempUploadInfo
    {
        public string TempId { get; set; } = string.Empty;
        public string TempFilePath { get; set; } = string.Empty;
        public string RelativeFilePath { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}