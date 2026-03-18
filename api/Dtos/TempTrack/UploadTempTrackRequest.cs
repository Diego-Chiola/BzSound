namespace api.Dtos.TempTrack;

public class UploadTempTrackRequest
{
    public IFormFile File { get; set; } = default!;
    public string? Title { get; set; }
}