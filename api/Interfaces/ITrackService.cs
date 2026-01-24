using api.Dtos.Track;
using api.Helpers;
using api.Models;

namespace api.Interfaces;

public interface ITrackService
{
    Task<IEnumerable<GetTrackRequest>> GetTracksByUserAsync(Guid userId, TrackQueryObject query);
    Task<GetTrackRequest?> GetTrackAsync(Guid userId, int trackId);
    Task<Track> CreateTrackAsync(Guid userId, CreateTrackRequest request);
    Task<UpdateTrackRequest?> UpdateTrackAsync(Guid userId, int trackId, UpdateTrackRequest request);
    Task<bool> DeleteTrackAsync(Guid userId, int trackId);
    Task<bool> UserExistsAsync(Guid userId);
}