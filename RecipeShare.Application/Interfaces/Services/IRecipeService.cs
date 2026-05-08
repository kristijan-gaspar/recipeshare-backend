using RecipeShare.Application.DTOs.Common;
using RecipeShare.Application.DTOs.Recipes;

namespace RecipeShare.Application.Interfaces.Services;

public interface IRecipeService
{
    Task<CursorPagedResponse<RecipeSummaryResponse>> GetRecipesAsync(RecipeQueryParameters parameters, int userId);
    Task<RecipeDetailResponse> GetRecipeByIdAsync(int id, int userId);
    Task<int> CreateAsync(CreateRecipeRequest request, int userId);
    Task UpdateAsync(int id, UpdateRecipeRequest request, int userId);
    Task DeleteAsync(int id, int userId);
    Task<string> UploadImageAsync(int id, Stream image, string fileName, int userId);
    Task DeleteImageAsync(int id, int userId);
}
