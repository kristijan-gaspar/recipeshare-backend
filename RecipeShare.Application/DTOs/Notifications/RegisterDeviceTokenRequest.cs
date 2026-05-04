using System.ComponentModel.DataAnnotations;

namespace RecipeShare.Application.DTOs.Notifications;

public class RegisterDeviceTokenRequest
{
    [Required]
    public string Token { get; set; } = null!;
}
