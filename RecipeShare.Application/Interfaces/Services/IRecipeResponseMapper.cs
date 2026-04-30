using RecipeShare.Application.DTOs.Common;
using RecipeShare.Application.DTOs.Recipes;
using RecipeShare.Domain.Entities;

namespace RecipeShare.Application.Interfaces.Services;

public interface IRecipeResponseMapper
{
    Task<CursorPagedResponse<RecipeSummaryResponse>> ToCursorPagedAsync(
        IEnumerable<Recipe> recipes,
        bool hasMore,
        int userId);
}
