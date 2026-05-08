using RecipeShare.Domain.Enums;

namespace RecipeShare.Application.DTOs.Recipes.Admin;

public class AdminRecipeListItemResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public double AverageRating { get; set; }
    public int RatingCount { get; set; }
}
