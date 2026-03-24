using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Track;

/**
 * This DTO is used for uploading a track file along with an optional title.
 * The file is required, while the title is optional and can be set to null or empty.
 * The title has a maximum length of 150 characters.
 */
public class UploadTrackRequest
{
    [MaxLength(150, ErrorMessage = "Title cannot exceed 150 characters.")]
    public string? Title { get; set; }

    [Required]
    public IFormFile File { get; set; } = default!;
}
