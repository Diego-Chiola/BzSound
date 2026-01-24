using api.Dtos.Track;
using api.Models;

namespace api.Mappers;

public static class TrackMappers
{
    public static GetTrackRequest ToGetTrackRequestFromTrack(this Track trackModel)
    {
        return new GetTrackRequest
        {
            Id = trackModel.Id,
            Title = trackModel.Title,
            LastModified = trackModel.LastModified,
            FilePath = trackModel.FilePath,
        };
    }
    public static Track ToTrackFromCreateTrackRequest(this CreateTrackRequest request, string userId)
    {
        return new Track
        {
            Title = request.Title,
            LastModified = DateTime.UtcNow,
            FilePath = request.FilePath,
            UserId = userId
        };
    }

    public static UpdateTrackRequest ToUpdateTrackRequestFromTrack(this Track existingTrack)
    {
        return new UpdateTrackRequest
        {
            Title = existingTrack.Title,
            FilePath = existingTrack.FilePath,
        };
    }
}