
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
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<TrackController> _logger;
    private const long MaxFileSize = 50 * 1024 * 1024; // 50MB
    private static readonly string[] AllowedExtensions = { ".mp3", ".wav", ".ogg", ".m4a", ".flac" };

    public TrackController(ITrackService trackService, IWebHostEnvironment environment, ILogger<TrackController> logger)
    {
        _trackService = trackService;
        _environment = environment;
        _logger = logger;
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

    /// <summary>
    /// Upload audio file and create track (requires authentication)
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadTrack(Guid userId, [FromForm] IFormFile file, [FromForm] string? title)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId != userId.ToString())
            return Forbid();

        if (!await _trackService.UserExistsAsync(userId))
            return BadRequest("User does not exist.");

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded." });

        if (file.Length > MaxFileSize)
            return BadRequest(new { message = $"File size exceeds maximum allowed size of {MaxFileSize / (1024 * 1024)}MB." });

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return BadRequest(new { message = $"Invalid file type. Allowed types: {string.Join(", ", AllowedExtensions)}" });

        try
        {
            // Create user-specific directory
            var userTracksPath = Path.Combine(_environment.ContentRootPath, "uploads", "tracks", userId.ToString());
            Directory.CreateDirectory(userTracksPath);

            // Generate unique filename
            var uniqueFileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(userTracksPath, uniqueFileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var trackTitle = string.IsNullOrWhiteSpace(title) ? Path.GetFileNameWithoutExtension(file.FileName) : title;
            var relativeFilePath = $"/uploads/tracks/{userId}/{uniqueFileName}";

            var createRequest = new CreateTrackRequest
            {
                Title = trackTitle,
                FilePath = relativeFilePath
            };

            var trackModel = await _trackService.CreateTrackAsync(userId, createRequest);
            return CreatedAtAction(nameof(GetTrack), new { userId, trackId = trackModel.Id }, trackModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading track file");
            return StatusCode(500, new { message = "Error uploading file." });
        }
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