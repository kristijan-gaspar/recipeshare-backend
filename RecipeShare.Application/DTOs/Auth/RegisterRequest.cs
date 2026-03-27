using System.ComponentModel.DataAnnotations;

namespace RecipeShare.Application.DTOs.Auth;

public class RegisterRequest
{
    [Required(ErrorMessage = "Username je obavezan")]
    [MinLength(3, ErrorMessage = "Username mora imati barem 3 znaka")]
    [MaxLength(30, ErrorMessage = "Username ne smije biti duži od 30 znakova")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email je obavezan")]
    [EmailAddress(ErrorMessage = "Email format nije valjan")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lozinka je obavezna")]
    [MinLength(6, ErrorMessage = "Lozinka mora imati barem 6 znakova")]
    public string Password { get; set; } = string.Empty;
}
