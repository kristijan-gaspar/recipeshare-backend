using System.ComponentModel.DataAnnotations;

namespace RecipeShare.Application.DTOs.Recipes;

public class StepRequest
{
    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public int Order { get; set; }
}
