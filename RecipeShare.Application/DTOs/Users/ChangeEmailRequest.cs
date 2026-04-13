using System.ComponentModel.DataAnnotations;

namespace RecipeShare.Application.DTOs.Users;

public class ChangeEmailRequest
{
    [Required]
    [EmailAddress]
    public string NewEmail { get; set; } = string.Empty;

    [Required]
    public string CurrentPassword { get; set; } = string.Empty;
}
