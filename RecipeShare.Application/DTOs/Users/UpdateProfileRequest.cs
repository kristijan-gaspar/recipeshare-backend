using System.ComponentModel.DataAnnotations;

namespace RecipeShare.Application.DTOs.Users;

public class UpdateProfileRequest
{
    [Required]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters long.")]
    [MaxLength(30, ErrorMessage = "Username can't be longer than 30 characters.")]
    public string Username { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Bio can't be longer than 500 characters.")]
    public string? Bio { get; set; }
}
