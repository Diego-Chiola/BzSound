using api.Dtos.User;
using api.Helpers;

namespace api.Interfaces;

public interface IUserService
{
    Task<IEnumerable<GetUserRequest>> GetAllUsersAsync(UserQueryObject query);
    Task<GetUserRequest?> GetUserByIdAsync(Guid id);
    Task<UpdateUserRequest?> UpdateUserAsync(Guid id, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(Guid id);
}