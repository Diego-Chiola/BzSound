using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class Track
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        public string FilePath { get; set; } = string.Empty;
        public string? UserId { get; set; }

        // Navigation property to owner
        [ForeignKey(nameof(UserId))]
        public AppUser User { get; set; } = null!;
    }
}