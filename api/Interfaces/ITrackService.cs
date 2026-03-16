using api.Dtos.Track;
using api.Helpers;
using api.Models;
using Microsoft.AspNetCore.Http;

namespace api.Interfaces;

public interface ITrackService
{
    // File validation
    FileValidationResult ValidateFile(IFormFile file);

    // Temporary track operations (for anonymous users)
    Task<TempUploadResponse> UploadTempTrackAsync(IFormFile file, string? title);
    Task<TempUploadResponse?> GetTempTrackAsync(string tempId);
    Task<TempUploadResponse?> UpdateTempTrackFileAsync(string tempId, IFormFile file);
    Task<bool> DeleteTempTrackAsync(string tempId);

    // Permanent track operations (requires authentication)
    Task<Track> SaveTempTrackAsync(Guid userId, SaveTempTrackRequest request);
    Task<IEnumerable<GetTrackRequest>> GetTracksByUserAsync(Guid userId, TrackQueryObject query);
    Task<GetTrackRequest?> GetTrackAsync(Guid userId, int trackId);
    Task<Track> CreateTrackAsync(Guid userId, CreateTrackRequest request);
    Task<UpdateTrackRequest?> UpdateTrackAsync(Guid userId, int trackId, UpdateTrackRequest request);
    Task<bool> DeleteTrackAsync(Guid userId, int trackId);

    // User validation
    Task<bool> UserExistsAsync(Guid userId);
}