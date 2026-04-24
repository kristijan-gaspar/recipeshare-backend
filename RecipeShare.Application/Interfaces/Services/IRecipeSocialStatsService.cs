using RecipeShare.Application.DTOs.Recipes;

namespace RecipeShare.Application.Interfaces.Services;

public interface IRecipeSocialStatsService
{
    Task ApplyStatsAsync(List<RecipeSummaryResponse> responses, int userId);
    Task ApplyStatsAsync(RecipeDetailResponse response, int userId);
}
