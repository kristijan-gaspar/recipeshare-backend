using System.ComponentModel.DataAnnotations;

namespace RecipeShare.Application.DTOs.Users;

public class UpdateProfileRequest
{
    [MaxLength(500, ErrorMessage = "Bio can't be longer than 500 characters.")]
    public string? Bio { get; set; }
}