using Microsoft.AspNetCore.Identity;

namespace api.Models;

public class AppUser : IdentityUser<Guid>
{
    public List<Track> UploadedTracks { get; set; } = new List<Track>();
}