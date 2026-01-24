using api.Dtos.User;
using api.Helpers;
using api.Interfaces;
using api.Mappers;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api.Service;

public class UserService : IUserService
{
    private readonly UserManager<AppUser> _userManager;

    public UserService(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IEnumerable<GetUserRequest>> GetAllUsersAsync(UserQueryObject query)
    {
        IQueryable<AppUser> usersQuery = _userManager.Users;

        int skip = (query.PageNumber - 1) * query.PageSize;

        var users = await usersQuery
            .Skip(skip)
            .Take(query.PageSize)
            .ToListAsync();

        return users.Select(u => u.ToGetUserRequest());
    }

    public async Task<GetUserRequest?> GetUserByIdAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        return user?.ToGetUserRequest();
    }

    public async Task<UpdateUserRequest?> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
            return null;

        if (!string.IsNullOrWhiteSpace(request.Email))
            user.Email = request.Email;

        await _userManager.UpdateAsync(user);

        return user.ToUpdateUserRequest();
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
            return false;

        await _userManager.DeleteAsync(user);
        return true;
    }
}