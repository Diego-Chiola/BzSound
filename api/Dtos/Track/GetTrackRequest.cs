using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Track;

/// <summary>
/// DTO for retrieving track information.
public class GetTrackRequest
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
    public string FilePath { get; set; } = string.Empty;
}