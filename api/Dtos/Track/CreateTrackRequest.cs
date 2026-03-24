using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Track;

/**
 * This DTO is used for creating a new track. It requires a title and a file path, along with the file size and duration.
 * The title must be between 1 and 150 characters, while the file path must be between 1 and 500 characters.
 */
public class CreateTrackRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "Title must be at least 1 character long.")]
    [MaxLength(150, ErrorMessage = "Title cannot exceed 150 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "FilePath must be at least 1 character long.")]
    [MaxLength(500, ErrorMessage = "FilePath cannot exceed 500 characters.")]
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public long Duration { get; set; }

    public CreateTrackRequest(string title, string filePath, long fileSize, long duration)
    {
        Title = title;
        FilePath = filePath;
        FileSize = fileSize;
        Duration = duration;
    }
}