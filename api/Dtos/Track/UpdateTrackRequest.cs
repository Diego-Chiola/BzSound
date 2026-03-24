using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Track;

public class UpdateTrackRequest
{
    [MinLength(1, ErrorMessage = "Title must be at least 1 character long.")]
    [MaxLength(150, ErrorMessage = "Title cannot exceed 150 characters.")]
    public string? Title { get; set; }

    public IFormFile? File { get; set; }
}