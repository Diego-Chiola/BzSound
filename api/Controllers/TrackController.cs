
using api.Dtos.Track;
using Microsoft.AspNetCore.Mvc;
using api.Helpers;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using api.Interfaces;

namespace api.Controllers;

[ApiController]
[Route("api/tracks")]
public class TrackController : ControllerBase
{
    private readonly ITrackService _trackService;

    public TrackController(ITrackService trackService)
    {
        _trackService = trackService;
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

        var result = await _trackService.CreateTrackWithFileAsync(userId, request);
        if (result.Status == TrackOperationStatus.UserNotFound)
            return BadRequest(result.ErrorMessage);

        if (result.Status == TrackOperationStatus.InvalidFile)
            return BadRequest(new { message = result.ErrorMessage });

        if (!result.IsSuccess || result.Data is null)
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to create track." });

        return CreatedAtAction(nameof(GetTrack), new { userId, trackId = result.Data.Id }, result.Data);
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

        var result = await _trackService.UpdateTrackWithOptionalFileAsync(userId, trackId, request);

        if (result.Status == TrackOperationStatus.InvalidRequest || result.Status == TrackOperationStatus.InvalidFile)
            return BadRequest(new { message = result.ErrorMessage });

        if (result.Status == TrackOperationStatus.TrackNotFound)
            return NotFound();

        if (!result.IsSuccess || result.Data is null)
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to update track." });

        return Ok(result.Data);
    }

    [Authorize]
    [HttpDelete("~/api/users/{userId:guid}/tracks/{trackId:int}")]
    public async Task<IActionResult> DeleteTrack(Guid userId, [FromRoute] int trackId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId != userId.ToString())
            return Forbid();

        var result = await _trackService.DeleteTrackWithFileAsync(userId, trackId);
        if (result.Status == TrackOperationStatus.TrackNotFound)
            return NotFound();

        if (!result.IsSuccess)
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to delete track." });

        return NoContent();
    }

}