namespace api.Dtos.Track;

public class UpdateTrackDataRequest
{
    public string? Title { get; set; }
    public string? FilePath { get; set; }
    public long? FileSize { get; set; }
    public string? Format { get; set; }
    public long? Duration { get; set; }
}
