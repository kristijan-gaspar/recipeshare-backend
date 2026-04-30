using RecipeShare.Application.DTOs.Common;
using RecipeShare.Application.DTOs.Recipes;

namespace RecipeShare.Application.Interfaces.Services;

public interface IFeedService
{
    Task<CursorPagedResponse<RecipeSummaryResponse>> GetFeedAsync(int userId, RecipeQueryParameters parameters);
    Task<CursorPagedResponse<RecipeSummaryResponse>> GetExploreAsync(int userId, RecipeQueryParameters parameters);
    Task<CursorPagedResponse<RecipeSummaryResponse>> GetFeaturedAsync(int userId, RecipeQueryParameters parameters);
}