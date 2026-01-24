using System.ComponentModel.DataAnnotations;

namespace api.Dtos.User;

public class UpdateUserRequest
{
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    public string? Email { get; set; }

    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    public string? PasswordHash { get; set; }
}