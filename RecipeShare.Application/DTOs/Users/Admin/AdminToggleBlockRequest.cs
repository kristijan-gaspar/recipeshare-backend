using System.ComponentModel.DataAnnotations;

namespace RecipeShare.Application.DTOs.Users.Admin;

public class AdminToggleBlockRequest
{
    [Required]
    public bool IsBlocked { get; set; }
}
