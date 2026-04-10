using System.ComponentModel.DataAnnotations;

namespace RecipeShare.Application.DTOs.Users;

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(8, ErrorMessage = "New password must be at least 8 characters long.")]
    public string NewPassword { get; set; } = string.Empty;
}
