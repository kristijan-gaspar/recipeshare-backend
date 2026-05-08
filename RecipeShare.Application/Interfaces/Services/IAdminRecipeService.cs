using RecipeShare.Application.Common;
using RecipeShare.Application.DTOs.Recipes.Admin;

namespace RecipeShare.Application.Interfaces.Services;

public interface IAdminRecipeService
{
    Task<PagedResponse<AdminRecipeListItemResponse>> GetRecipesAsync(AdminRecipeListQuery query);
    Task<AdminRecipeDetailResponse> GetRecipeAsync(int recipeId);
    Task SoftDeleteAsync(int recipeId);
    Task RestoreAsync(int recipeId);
    Task ToggleFeaturedAsync(int recipeId);
}
