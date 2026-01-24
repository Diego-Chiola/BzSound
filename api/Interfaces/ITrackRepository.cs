using api.Dtos.Track;
using api.Helpers;
using api.Models;

namespace api.Interfaces;

public interface ITrackRepository
{
    Task<IEnumerable<Track>> GetTracksByUserAsync(Guid userId, TrackQueryObject query);
    Task<Track?> GetTrackAsync(int id);
    Task<Track> CreateTrackAsync(Track track);
    Task<Track?> UpdateTrackAsync(Guid userId, int trackId, UpdateTrackRequest newTrack);
    Task<Track?> DeleteTrackAsync(Guid userId, int trackId);
}