using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Track;

public class UpdateTrackRequest
{
    [MinLength(1, ErrorMessage = "Title must be at least 1 character long.")]
    [MaxLength(150, ErrorMessage = "Title cannot exceed 150 characters.")]
    public string? Title { get; set; }

    [MinLength(1, ErrorMessage = "FilePath must be at least 1 character long.")]
    [MaxLength(500, ErrorMessage = "FilePath cannot exceed 500 characters.")]
    public string? FilePath { get; set; }
}