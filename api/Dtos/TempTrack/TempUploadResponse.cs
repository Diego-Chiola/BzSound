namespace api.Dtos.TempTrack;

public class TempUploadResponse
{
    public string TempId { get; set; } = string.Empty;
    public string TempAccessToken { get; set; } = string.Empty;
    public string TempFilePath { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
