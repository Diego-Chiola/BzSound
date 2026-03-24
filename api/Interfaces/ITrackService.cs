using api.Dtos.Track;
using api.Helpers;
using api.Models;
using Microsoft.AspNetCore.Http;

namespace api.Interfaces;

public interface ITrackService
{
    // Permanent track operations (requires authentication)
    Task<bool> UserExistsAsync(Guid userId);
    Task<IEnumerable<GetTrackRequest>> GetTracksByUserAsync(Guid userId, TrackQueryObject query);
    Task<GetTrackRequest?> GetTrackAsync(Guid userId, int trackId);
    Task<Track> CreateTrackAsync(Guid userId, CreateTrackRequest request);
    Task<GetTrackRequest?> UpdateTrackAsync(Guid userId, int trackId, UpdateTrackDataRequest request);
    Task<bool> DeleteTrackAsync(Guid userId, int trackId);
}