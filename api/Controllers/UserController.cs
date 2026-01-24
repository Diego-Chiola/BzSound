using api.Dtos.User;
using api.Helpers;
using api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace api.Controllers;

[Authorize]
[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers([FromQuery] UserQueryObject query)
    {
        var users = await _userService.GetAllUsersAsync(query);

        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        Console.WriteLine($"Current User ID from token: {currentUserId}");
        Console.WriteLine($"Requested ID: {id}");
        Console.WriteLine($"Match: {currentUserId == id.ToString()}");

        if (currentUserId != id.ToString() && !User.IsInRole("Admin"))
            return Forbid();

        var user = await _userService.GetUserByIdAsync(id);
        if (user is null)
            return NotFound();

        return Ok(user);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        Console.WriteLine($"Current User ID from token: {currentUserId}");

        if (currentUserId is null)
            return Unauthorized();

        var user = await _userService.GetUserByIdAsync(Guid.Parse(currentUserId));
        if (user is null)
            return NotFound();

        return Ok(user);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (currentUserId != id.ToString() && !User.IsInRole("Admin"))
            return Forbid();

        var updatedUser = await _userService.UpdateUserAsync(id, request);
        if (updatedUser is null)
            return NotFound();

        return Ok(updatedUser);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (currentUserId != id.ToString() && !User.IsInRole("Admin"))
            return Forbid();

        var deletedUser = await _userService.DeleteUserAsync(id);
        if (!deletedUser)
            return NotFound();

        return NoContent();
    }
}