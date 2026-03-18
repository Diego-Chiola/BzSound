using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.Interfaces;
using System.Security.Claims;
using api.Dtos.TempTrack;

namespace api.Controllers;

[ApiController]
[Route("api/tracks")]
public class TempTrackController : ControllerBase
{
    private const string TempAccessTokenHeader = "X-Temp-Access-Token";

    private readonly ITempTrackService _tempTrackService;
    private readonly ITrackService _trackService;
    private readonly ILogger<TempTrackController> _logger;
    private readonly IFileValidationService _fileValidationService;

    public TempTrackController(
        ITempTrackService tempTrackService,
        ITrackService trackService,
        ILogger<TempTrackController> logger,
        IFileValidationService fileValidationService)
    {
        _tempTrackService = tempTrackService;
        _trackService = trackService;
        _logger = logger;
        _fileValidationService = fileValidationService;
    }

    /// <summary>
    /// Upload a temporary audio file (no authentication required)
    /// File will be stored temporarily and can be saved permanently after authentication
    /// </summary>
    [AllowAnonymous]
    [HttpPost("temp/upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadTempTrack([FromForm] UploadTempTrackRequest request)
    {
        var validation = _fileValidationService.ValidateFile(request.File);
        if (!validation.IsValid)
            return BadRequest(new { message = validation.ErrorMessage });

        try
        {
            var result = await _tempTrackService.UploadTempTrackAsync(request.File, request.Title);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading temporary track file");
            return StatusCode(500, new { message = "Error uploading file." });
        }
    }

    /// <summary>
    /// Get temporary track info by temp ID (requires access token)
    /// </summary>
    [AllowAnonymous]
    [HttpGet("temp/{tempId}")]
    public async Task<IActionResult> GetTempTrack(
        string tempId,
        [FromHeader(Name = TempAccessTokenHeader)] string tempAccessToken)
    {
        if (string.IsNullOrWhiteSpace(tempAccessToken))
            return Unauthorized(new { message = $"Missing required header: {TempAccessTokenHeader}" });

        var tempTrack = await _tempTrackService.GetTempTrackAsync(tempId, tempAccessToken);
        if (tempTrack == null)
            return NotFound(new { message = "Temporary track not found or expired." });

        return Ok(tempTrack);
    }

    /// <summary>
    /// Update/replace a temporary track file (requires access token)
    /// </summary>
    [AllowAnonymous]
    [HttpPut("temp/{tempId}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateTempTrack(
        string tempId,
        [FromHeader(Name = TempAccessTokenHeader)] string tempAccessToken,
        [FromForm] UpdateTempTrackFileRequest request)
    {
        if (string.IsNullOrWhiteSpace(tempAccessToken))
            return Unauthorized(new { message = $"Missing required header: {TempAccessTokenHeader}" });

        var validation = _fileValidationService.ValidateFile(request.File);
        if (!validation.IsValid)
            return BadRequest(new { message = validation.ErrorMessage });

        try
        {
            var result = await _tempTrackService.UpdateTempTrackFileAsync(tempId, request.File, tempAccessToken);
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
    /// Delete a temporary track (requires access token)
    /// </summary>
    [AllowAnonymous]
    [HttpDelete("temp/{tempId}")]
    public async Task<IActionResult> DeleteTempTrack(
        string tempId,
        [FromHeader(Name = TempAccessTokenHeader)] string tempAccessToken)
    {
        if (string.IsNullOrWhiteSpace(tempAccessToken))
            return Unauthorized(new { message = $"Missing required header: {TempAccessTokenHeader}" });

        var deleted = await _tempTrackService.DeleteTempTrackAsync(tempId, tempAccessToken);
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

        // Ensure the tempId in route matches the request
        request.TempId = tempId;

        try
        {
            var track = await _tempTrackService.SaveTempTrackAsync(userId, request);
            return CreatedAtAction("GetTrack", "Track", new { userId, trackId = track.Id }, track);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
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

}
