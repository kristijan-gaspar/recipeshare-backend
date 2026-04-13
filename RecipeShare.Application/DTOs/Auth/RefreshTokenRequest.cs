using System.ComponentModel.DataAnnotations;

namespace RecipeShare.Application.DTOs.Auth;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
