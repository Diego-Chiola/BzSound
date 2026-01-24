using Microsoft.AspNetCore.Identity;

namespace api.Models;

public class AppUser : IdentityUser
{
    public List<Track> UploadedTracks { get; set; } = new List<Track>();
}