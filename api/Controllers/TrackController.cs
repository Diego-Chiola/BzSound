
using api.Dtos.Track;
using Microsoft.AspNetCore.Mvc;
using api.Helpers;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using api.Interfaces;

namespace api.Controllers;

[Authorize]
[ApiController]
[Route("api/users/{userId:guid}/tracks")]
public class TrackController : ControllerBase
{
    private readonly ITrackService _trackService;

    public TrackController(ITrackService trackService)
    {
        _trackService = trackService;
    }

    [HttpGet]
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

    /*** Added method to get track by its ID ***/
    [HttpGet("{trackId:int}")]
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

    [HttpPost]
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

    [HttpPut("{trackId:int}")]
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

    [HttpDelete("{trackId:int}")]
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