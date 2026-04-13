using System.ComponentModel.DataAnnotations;
using RecipeShare.Domain.Enums;

namespace RecipeShare.Application.DTOs.Recipes;

public class UpdateRecipeRequest
{
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Range(1, 1440)]
    public int PrepTimeMinutes { get; set; }

    [Range(0, 1440)]
    public int CookTimeMinutes { get; set; }

    [Range(1, 100)]
    public int Servings { get; set; }

    [Required]
    public DifficultyLevel Difficulty { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    public List<int> TagIds { get; set; } = new();

    [Required]
    [MinLength(1)]
    public List<IngredientRequest> Ingredients { get; set; } = new();

    [Required]
    [MinLength(1)]
    public List<StepRequest> Steps { get; set; } = new();
}
