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
    private readonly IFileService _fileService;

    public TrackService(
        ITrackRepository trackRepository,
        UserManager<AppUser> userManager,
        IFileService fileService)
    {
        _trackRepository = trackRepository;
        _userManager = userManager;
        _fileService = fileService;
    }

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

    public async Task<TrackOperationResult<GetTrackRequest>> CreateTrackWithFileAsync(Guid userId, UploadTrackRequest request)
    {
        if (!await UserExistsAsync(userId))
            return TrackOperationResult<GetTrackRequest>.Failure(
                TrackOperationStatus.UserNotFound,
                "User does not exist.");

        var fileValidation = _fileService.ValidateAudioFile(request.File);
        if (!fileValidation.IsValid)
            return TrackOperationResult<GetTrackRequest>.Failure(
                TrackOperationStatus.InvalidFile,
                fileValidation.ErrorMessage);

        var title = request.Title;
        if (string.IsNullOrWhiteSpace(title))
            title = Path.GetFileNameWithoutExtension(request.File.FileName);

        var relativeFilePath = await _fileService.SaveFileAsync(request.File, userId.ToString(), title);

        var newTrackRequest = new CreateTrackRequest(
            title: title,
            filePath: relativeFilePath,
            fileSize: request.File.Length,
            duration: await _fileService.GetAudioDurationSecondsAsync(request.File) ?? 0);

        var trackModel = await CreateTrackAsync(userId, newTrackRequest);

        return TrackOperationResult<GetTrackRequest>.Success(trackModel.ToGetTrackRequestFromTrack());
    }

    public async Task<TrackOperationResult<GetTrackRequest>> UpdateTrackWithOptionalFileAsync(Guid userId, int trackId, UpdateTrackRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) && request.File is null)
            return TrackOperationResult<GetTrackRequest>.Failure(
                TrackOperationStatus.InvalidRequest,
                "At least one field is required to update (title or file).");

        var currentTrack = await GetTrackAsync(userId, trackId);
        if (currentTrack == null)
            return TrackOperationResult<GetTrackRequest>.Failure(
                TrackOperationStatus.TrackNotFound,
                null);

        var updateData = new UpdateTrackDataRequest
        {
            Title = request.Title
        };

        var oldFilePath = currentTrack.FilePath;

        if (request.File is not null)
        {
            var fileValidation = _fileService.ValidateAudioFile(request.File);
            if (!fileValidation.IsValid)
                return TrackOperationResult<GetTrackRequest>.Failure(
                    TrackOperationStatus.InvalidFile,
                    fileValidation.ErrorMessage);

            var fileTitle = string.IsNullOrWhiteSpace(request.Title)
                ? currentTrack.Title
                : request.Title;

            updateData.FilePath = await _fileService.SaveFileAsync(request.File, userId.ToString(), fileTitle);
            updateData.FileSize = request.File.Length;
            updateData.Format = Path.GetExtension(request.File.FileName).ToLowerInvariant();
            updateData.Duration = await _fileService.GetAudioDurationSecondsAsync(request.File) ?? 0;

            var updatedTrackWithFile = await UpdateTrackAsync(userId, trackId, updateData);
            if (updatedTrackWithFile == null)
            {
                await _fileService.DeleteFileAsync(updateData.FilePath!);
                return TrackOperationResult<GetTrackRequest>.Failure(
                    TrackOperationStatus.TrackNotFound,
                    null);
            }

            await _fileService.DeleteFileAsync(oldFilePath);
            return TrackOperationResult<GetTrackRequest>.Success(updatedTrackWithFile);
        }

        var updatedTrack = await UpdateTrackAsync(userId, trackId, updateData);
        if (updatedTrack == null)
            return TrackOperationResult<GetTrackRequest>.Failure(
                TrackOperationStatus.TrackNotFound,
                null);

        return TrackOperationResult<GetTrackRequest>.Success(updatedTrack);
    }

    public async Task<TrackOperationResult> DeleteTrackWithFileAsync(Guid userId, int trackId)
    {
        var track = await GetTrackAsync(userId, trackId);
        if (track == null)
            return TrackOperationResult.Failure(TrackOperationStatus.TrackNotFound, null);

        var deletedTrack = await DeleteTrackAsync(userId, trackId);
        if (!deletedTrack)
            return TrackOperationResult.Failure(TrackOperationStatus.TrackNotFound, null);

        await _fileService.DeleteFileAsync(track.FilePath);

        return TrackOperationResult.Success();
    }

    public async Task<Track> CreateTrackAsync(Guid userId, CreateTrackRequest request)
    {
        var trackModel = await _trackRepository.CreateTrackAsync(
            request.ToTrackFromCreateTrackRequest(userId));

        return trackModel;
    }

    public async Task<GetTrackRequest?> UpdateTrackAsync(Guid userId, int trackId, UpdateTrackDataRequest request)
    {
        var existingTrack = await _trackRepository.GetTrackAsync(trackId);

        if (existingTrack is null || existingTrack.UserId != userId)
            return null;

        var updatedTrack = await _trackRepository.UpdateTrackAsync(userId, trackId, request);
        return updatedTrack?.ToGetTrackRequestFromTrack();
    }

    public async Task<bool> DeleteTrackAsync(Guid userId, int trackId)
    {
        var track = await _trackRepository.DeleteTrackAsync(userId, trackId);
        return track is not null;
    }

    #endregion
}
