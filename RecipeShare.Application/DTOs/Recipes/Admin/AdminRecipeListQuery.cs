using System.ComponentModel.DataAnnotations;

namespace RecipeShare.Application.DTOs.Recipes.Admin;

public class AdminRecipeListQuery
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public bool? IsDeleted { get; set; }
    public bool? IsFeatured { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0.")]
    public int PageNumber { get; set; } = 1;

    [Range(1, int.MaxValue, ErrorMessage = "Page size must be greater than 0.")]
    public int PageSize { get; set; } = 20;
}
