using RecipeShare.Domain.Enums;

namespace RecipeShare.Application.DTOs.Recipes;

public class RecipeSummaryResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public RecipeAuthorResponse Author { get; set; } = null!;
    public string CategoryName { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public DifficultyLevel Difficulty { get; set; }
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; }
}
