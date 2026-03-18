using api.Dtos.TempTrack;
using api.Models;

namespace api.Interfaces;

public interface ITempTrackService
{
    // Temporary track operations
    Task<TempUploadResponse> UploadTempTrackAsync(IFormFile file, string? title);
    Task<TempUploadResponse?> GetTempTrackAsync(string tempId, string tempAccessToken);
    Task<TempUploadResponse?> UpdateTempTrackFileAsync(string tempId, IFormFile file, string tempAccessToken);
    Task<bool> DeleteTempTrackAsync(string tempId, string tempAccessToken);
    Task<Track> SaveTempTrackAsync(Guid userId, SaveTempTrackRequest request);
}
