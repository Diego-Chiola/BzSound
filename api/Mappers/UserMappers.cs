using api.Dtos.User;
using api.Models;

namespace api.Mappers;

public static class UserMappers
{
    public static GetUserRequest ToGetUserRequest(this AppUser user)
    {
        return new GetUserRequest
        {
            Id = user.Id,
            Email = user.Email,
            UploadedTracks = user.UploadedTracks.Select(t => t.ToGetTrackRequestFromTrack()).ToList()
        };
    }

    public static UpdateUserRequest ToUpdateUserRequest(this AppUser updateUserRequest)
    {
        return new UpdateUserRequest
        {
            Email = updateUserRequest.Email,
            PasswordHash = updateUserRequest.PasswordHash
        };
    }
}