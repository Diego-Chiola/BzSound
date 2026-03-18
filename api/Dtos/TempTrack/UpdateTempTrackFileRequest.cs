namespace api.Dtos.TempTrack;

public class UpdateTempTrackFileRequest
{
    public IFormFile File { get; set; } = default!;
}