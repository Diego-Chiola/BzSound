using System.ComponentModel.DataAnnotations;

namespace api.Dtos.User;

public class UpdateUserRequest
{
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    public string? Email { get; set; }
}