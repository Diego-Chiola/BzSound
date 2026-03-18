
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
    public async Task<IActionResult> CreateTrack(Guid userId, [FromBody] CreateTrackRequest newTrack)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId != userId.ToString())
            return Forbid();

        if (!await _trackService.UserExistsAsync(userId))
            return BadRequest("User does not exist.");

        var trackModel = await _trackService.CreateTrackAsync(userId, newTrack);
        return CreatedAtAction(nameof(GetTrack), new { userId, trackId = trackModel.Id }, trackModel);
    }

    [Authorize]
    [HttpPut("~/api/users/{userId:guid}/tracks/{trackId:int}")]
    public async Task<IActionResult> UpdateTrack(Guid userId, int trackId, [FromBody] UpdateTrackRequest newTrack)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId != userId.ToString())
            return Forbid();

        var existingTrack = await _trackService.UpdateTrackAsync(userId, trackId, newTrack);

        if (existingTrack == null)
            return NotFound();

        return Ok(existingTrack);
    }

    [Authorize]
    [HttpDelete("~/api/users/{userId:guid}/tracks/{trackId:int}")]
    public async Task<IActionResult> DeleteTrack(Guid userId, [FromRoute] int trackId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId != userId.ToString())
            return Forbid();

        var deletedTrack = await _trackService.DeleteTrackAsync(userId, trackId);
        if (!deletedTrack)
            return NotFound();

        return NoContent();
    }
}