namespace RecipeShare.Application.DTOs.Recipes.Admin;

public class AdminRecipeListQuery
{
    public string? Query { get; set; }
    public int? CategoryId { get; set; }
    public bool? IsDeleted { get; set; }
    public bool? IsFeatured { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
