using RecipeShare.Domain.Enums;

namespace RecipeShare.Application.DTOs.Recipes;

public class RecipeQueryParameters
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public List<int>? TagIds { get; set; }
    public DifficultyLevel? Difficulty { get; set; }
    public int? Cursor { get; set; }
    public int PageSize { get; set; } = 10;
}
