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
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
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

        if (!User.IsInRole("Admin") && (currentUserId is null || currentUserId != id.ToString()))
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

        if (currentUserId is null || currentUserId != id.ToString())
            return Forbid();

        var updatedUser = await _userService.UpdateUserAsync(id, request);
        if (updatedUser is null)
            return NotFound();

        return Ok(updatedUser);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUserAsAdmin(Guid id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!User.IsInRole("Admin"))
            return Forbid();

        if (currentUserId == id.ToString())
            return BadRequest(new { message = "Admins cannot delete their own account from this endpoint. Use DELETE /api/users/me instead." });

        var deletedUser = await _userService.DeleteUserAsync(id);
        if (!deletedUser)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("me")]
    public async Task<IActionResult> DeleteCurrentUser()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
            return Unauthorized();

        var deletedUser = await _userService.DeleteUserAsync(Guid.Parse(currentUserId));
        if (!deletedUser)
            return NotFound();

        return NoContent();
    }
}