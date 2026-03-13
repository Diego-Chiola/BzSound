
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
    private readonly ILogger<TrackController> _logger;

    public TrackController(ITrackService trackService, ILogger<TrackController> logger)
    {
        _trackService = trackService;
        _logger = logger;
    }

    #region Anonymous Endpoints (Temporary Track Operations)

    /// <summary>
    /// Upload a temporary audio file (no authentication required)
    /// File will be stored temporarily and can be saved permanently after authentication
    /// </summary>
    [AllowAnonymous]
    [HttpPost("temp/upload")]
    public async Task<IActionResult> UploadTempTrack([FromForm] IFormFile file, [FromForm] string? title)
    {
        var validation = _trackService.ValidateFile(file);
        if (!validation.IsValid)
            return BadRequest(new { message = validation.ErrorMessage });

        try
        {
            var result = await _trackService.UploadTempTrackAsync(file, title);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading temporary track file");
            return StatusCode(500, new { message = "Error uploading file." });
        }
    }

    /// <summary>
    /// Get temporary track info by temp ID (no authentication required)
    /// </summary>
    [AllowAnonymous]
    [HttpGet("temp/{tempId}")]
    public async Task<IActionResult> GetTempTrack(string tempId)
    {
        var tempTrack = await _trackService.GetTempTrackAsync(tempId);
        if (tempTrack == null)
            return NotFound(new { message = "Temporary track not found or expired." });

        return Ok(tempTrack);
    }

    /// <summary>
    /// Update/replace a temporary track file (no authentication required)
    /// </summary>
    [AllowAnonymous]
    [HttpPut("temp/{tempId}")]
    public async Task<IActionResult> UpdateTempTrack(string tempId, [FromForm] IFormFile file)
    {
        var validation = _trackService.ValidateFile(file);
        if (!validation.IsValid)
            return BadRequest(new { message = validation.ErrorMessage });

        try
        {
            var result = await _trackService.UpdateTempTrackFileAsync(tempId, file);
            if (result == null)
                return NotFound(new { message = "Temporary track not found or expired." });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating temporary track file");
            return StatusCode(500, new { message = "Error updating file." });
        }
    }

    /// <summary>
    /// Delete a temporary track (no authentication required)
    /// </summary>
    [AllowAnonymous]
    [HttpDelete("temp/{tempId}")]
    public async Task<IActionResult> DeleteTempTrack(string tempId)
    {
        var deleted = await _trackService.DeleteTempTrackAsync(tempId);
        if (!deleted)
            return NotFound(new { message = "Temporary track not found." });

        return NoContent();
    }

    /// <summary>
    /// Save a temporary track permanently (requires authentication)
    /// </summary>
    [Authorize]
    [HttpPost("temp/{tempId}/save")]
    public async Task<IActionResult> SaveTempTrack(string tempId, [FromBody] SaveTempTrackRequest request)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out var userId))
            return Unauthorized(new { message = "User not authenticated." });

        if (!await _trackService.UserExistsAsync(userId))
            return BadRequest(new { message = "User does not exist." });

        // Ensure the tempId in route matches the request
        request.TempId = tempId;

        try
        {
            var track = await _trackService.SaveTempTrackAsync(userId, request);
            return CreatedAtAction(nameof(GetTrack), new { userId, trackId = track.Id }, track);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving temporary track");
            return StatusCode(500, new { message = "Error saving track." });
        }
    }

    #endregion

    #region Authenticated Endpoints (Permanent Track Operations)

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

    /// <summary>
    /// Upload and save audio file directly (requires authentication)
    /// </summary>
    [Authorize]
    [HttpPost("~/api/users/{userId:guid}/tracks/upload")]
    public async Task<IActionResult> UploadTrack(Guid userId, [FromForm] IFormFile file, [FromForm] string? title)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId != userId.ToString())
            return Forbid();

        if (!await _trackService.UserExistsAsync(userId))
            return BadRequest("User does not exist.");

        var validation = _trackService.ValidateFile(file);
        if (!validation.IsValid)
            return BadRequest(new { message = validation.ErrorMessage });

        try
        {
            // Upload to temp first, then save permanently
            var tempResult = await _trackService.UploadTempTrackAsync(file, title);
            var saveRequest = new SaveTempTrackRequest
            {
                TempId = tempResult.TempId,
                Title = tempResult.Title
            };

            var track = await _trackService.SaveTempTrackAsync(userId, saveRequest);
            return CreatedAtAction(nameof(GetTrack), new { userId, trackId = track.Id }, track);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading track file");
            return StatusCode(500, new { message = "Error uploading file." });
        }
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

    #endregion
}