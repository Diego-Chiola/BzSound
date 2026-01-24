using api.Dtos.Track;

namespace api.Dtos.User;

public class GetUserRequest
{
    public Guid? Id { get; set; }
    public string? Email { get; set; }
    public List<GetTrackRequest> UploadedTracks { get; set; } = new List<GetTrackRequest>();
}