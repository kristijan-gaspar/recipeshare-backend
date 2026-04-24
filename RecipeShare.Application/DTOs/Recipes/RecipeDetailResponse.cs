using RecipeShare.Domain.Enums;

namespace RecipeShare.Application.DTOs.Recipes;

public class RecipeDetailResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public RecipeAuthorResponse Author { get; set; } = null!;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public DifficultyLevel Difficulty { get; set; }
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public int Servings { get; set; }
    public bool IsFeatured { get; set; }
    public List<IngredientResponse> Ingredients { get; set; } = new();
    public List<StepResponse> Steps { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int LikeCount { get; set; }
    public bool IsLikedByMe { get; set; }
    public double AverageRating { get; set; }
    public int RatingCount { get; set; }
    public int? MyRating { get; set; }
    public int CommentCount { get; set; }
}
