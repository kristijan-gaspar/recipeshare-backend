using RecipeShare.Application.DTOs.Comments.Admin;
using RecipeShare.Application.DTOs.Recipes;
using RecipeShare.Domain.Enums;

namespace RecipeShare.Application.DTOs.Recipes.Admin;

public class AdminRecipeDetailResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public int Servings { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public RecipeAuthorResponse Author { get; set; } = null!;
    public List<string> Tags { get; set; } = [];
    public List<IngredientResponse> Ingredients { get; set; } = [];
    public List<StepResponse> Steps { get; set; } = [];
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public double AverageRating { get; set; }
    public int RatingCount { get; set; }
    public List<AdminRecipeCommentItem> Comments { get; set; } = [];
}
