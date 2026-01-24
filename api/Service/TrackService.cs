using api.Dtos.Track;
using api.Helpers;
using api.Interfaces;
using api.Mappers;
using api.Models;
using Microsoft.AspNetCore.Identity;

namespace api.Services;

public class TrackService : ITrackService
{
    private readonly ITrackRepository _trackRepository;
    private readonly UserManager<AppUser> _userManager;

    public TrackService(ITrackRepository trackRepository, UserManager<AppUser> userManager)
    {
        _trackRepository = trackRepository;
        _userManager = userManager;
    }

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

        if (track is null || track.UserId != userId.ToString())
            return null;

        return track.ToGetTrackRequestFromTrack();
    }

    public async Task<Track> CreateTrackAsync(Guid userId, CreateTrackRequest request)
    {
        var trackModel = await _trackRepository.CreateTrackAsync(
            request.ToTrackFromCreateTrackRequest(userId.ToString()));

        return trackModel;
    }

    public async Task<UpdateTrackRequest?> UpdateTrackAsync(Guid userId, int trackId, UpdateTrackRequest request)
    {
        var existingTrack = await _trackRepository.GetTrackAsync(trackId);

        if (existingTrack is null || existingTrack.UserId != userId.ToString())
            return null;

        var updatedTrack = await _trackRepository.UpdateTrackAsync(userId, trackId, request);
        return updatedTrack?.ToUpdateTrackRequestFromTrack();
    }

    public async Task<bool> DeleteTrackAsync(Guid userId, int trackId)
    {
        var track = await _trackRepository.DeleteTrackAsync(userId, trackId);
        return track is not null;
    }
}