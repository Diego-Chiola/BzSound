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
            FileSize = trackModel.FileSize,
            Format = trackModel.Format,
            Duration = trackModel.Duration,
        };
    }
    public static Track ToTrackFromCreateTrackRequest(this CreateTrackRequest request, Guid userId)
    {
        return new Track
        {
            Title = request.Title,
            LastModified = DateTime.UtcNow,
            FilePath = request.FilePath,
            FileSize = request.FileSize,
            Format = Path.GetExtension(request.FilePath).ToLowerInvariant(),
            Duration = request.Duration,
            UserId = userId
        };
    }
}