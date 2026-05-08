using System.ComponentModel.DataAnnotations;

namespace RecipeShare.Application.DTOs.Users;

public class DeleteAccountRequest
{
    [Required]
    public string Password { get; set; } = null!;
}
