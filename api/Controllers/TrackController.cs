
using api.Dtos.Track;
using Microsoft.AspNetCore.Mvc;
using api.Helpers;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using api.Interfaces;
using api.Mappers;

namespace api.Controllers;

[ApiController]
[Route("api/tracks")]
public class TrackController : ControllerBase
{
    private readonly ITrackService _trackService;
    private readonly IFileService _fileService;

    public TrackController(
        ITrackService trackService,
        IFileService fileService)
    {
        _trackService = trackService;
        _fileService = fileService;
    }

    [Authorize]
    [HttpGet("~/api/users/{userId:guid}/tracks")]
    public async Task<IActionResult> GetTracksByUser(Guid userId, [FromQuery] TrackQueryObject query)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId != userId.ToString())
            return Forbid();

        if (!await _trackService.UserExistsAsync(userId))
            return NotFound("User not found.");

        var track = await _trackService.GetTracksByUserAsync(userId, query);

        return Ok(track);
    }

    [Authorize]
    [HttpGet("~/api/users/{userId:guid}/tracks/{trackId:int}")]
    public async Task<IActionResult> GetTrack(Guid userId, int trackId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId != userId.ToString())
            return Forbid();

        if (!await _trackService.UserExistsAsync(userId))
            return NotFound("User not found.");

        var track = await _trackService.GetTrackAsync(userId, trackId);
        if (track == null)
            return NotFound();

        return Ok(track);
    }

    [Authorize]
    [HttpPost("~/api/users/{userId:guid}/tracks")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateTrack(
        Guid userId,
        [FromForm] UploadTrackRequest request)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId != userId.ToString())
            return Forbid();

        if (!await _trackService.UserExistsAsync(userId))
            return BadRequest("User does not exist.");

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var file = request.File;

        var fileValidation = _fileService.ValidateAudioFile(file);
        if (!fileValidation.IsValid)
            return BadRequest(new { message = fileValidation.ErrorMessage });

        var title = request.Title;
        if (string.IsNullOrWhiteSpace(title))
            title = Path.GetFileNameWithoutExtension(file.FileName);

        var relativeFilePath = await _fileService.SaveFileAsync(file, userId.ToString(), title);

        var newTrack = new CreateTrackRequest(
            title: title,
            filePath: relativeFilePath,
            fileSize: file.Length,
            duration: await _fileService.GetAudioDurationSecondsAsync(file) ?? 0);

        var trackModel = await _trackService.CreateTrackAsync(userId, newTrack);
        return CreatedAtAction(nameof(GetTrack), new { userId, trackId = trackModel.Id }, trackModel.ToGetTrackRequestFromTrack());
    }

    [Authorize]
    [HttpPut("~/api/users/{userId:guid}/tracks/{trackId:int}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateTrack(Guid userId, int trackId, [FromForm] UpdateTrackRequest request)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId != userId.ToString())
            return Forbid();

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (string.IsNullOrWhiteSpace(request.Title) && request.File is null)
            return BadRequest(new { message = "At least one field is required to update (title or file)." });

        var currentTrack = await _trackService.GetTrackAsync(userId, trackId);
        if (currentTrack == null)
            return NotFound();

        var updateData = new UpdateTrackDataRequest
        {
            Title = request.Title
        };

        var oldFilePath = currentTrack.FilePath;

        // If a new file is provided, save it and delete the old one
        if (request.File is not null)
        {
            var fileValidation = _fileService.ValidateAudioFile(request.File);
            if (!fileValidation.IsValid)
                return BadRequest(new { message = fileValidation.ErrorMessage });

            // Use current title if not updating, otherwise use new title
            var fileTitle = string.IsNullOrWhiteSpace(request.Title)
                ? currentTrack.Title
                : request.Title;

            updateData.FilePath = await _fileService.SaveFileAsync(request.File, userId.ToString(), fileTitle);
            updateData.FileSize = request.File.Length;
            updateData.Format = Path.GetExtension(request.File.FileName).ToLowerInvariant();
            updateData.Duration = await _fileService.GetAudioDurationSecondsAsync(request.File) ?? 0;

            var updatedTrack = await _trackService.UpdateTrackAsync(userId, trackId, updateData);
            if (updatedTrack != null)
                await _fileService.DeleteFileAsync(oldFilePath);
            else
            {
                await _fileService.DeleteFileAsync(updateData.FilePath!);
                return NotFound();
            }

            return Ok(updatedTrack);
        }

        // If only title is being updated, just update the database
        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            var updatedTrack = await _trackService.UpdateTrackAsync(userId, trackId, updateData);
            if (updatedTrack == null)
                return NotFound();

            return Ok(updatedTrack);
        }

        return NotFound();
    }

    [Authorize]
    [HttpDelete("~/api/users/{userId:guid}/tracks/{trackId:int}")]
    public async Task<IActionResult> DeleteTrack(Guid userId, [FromRoute] int trackId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId != userId.ToString())
            return Forbid();

        var track = await _trackService.GetTrackAsync(userId, trackId);
        if (track == null)
            return NotFound();

        var deletedTrack = await _trackService.DeleteTrackAsync(userId, trackId);
        if (!deletedTrack)
            return NotFound();

        await _fileService.DeleteFileAsync(track.FilePath);

        return NoContent();
    }

}