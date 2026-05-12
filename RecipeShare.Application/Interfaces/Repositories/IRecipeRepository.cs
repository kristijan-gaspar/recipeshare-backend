using RecipeShare.Application.DTOs.Recipes;
using RecipeShare.Application.DTOs.Recipes.Admin;
using RecipeShare.Domain.Entities;

namespace RecipeShare.Application.Interfaces.Repositories;

public interface IRecipeRepository : IGenericRepository<Recipe>
{
    Task<(IEnumerable<Recipe> Items, bool HasMore)> GetCursorPagedAsync(RecipeQueryParameters parameters);
    Task<Recipe?> GetDetailedByIdAsync(int id);
    Task<bool> IsAuthorAsync(int recipeId, int userId);
    void RemoveIngredients(IEnumerable<Ingredient> ingredients);
    void RemoveSteps(IEnumerable<Step> steps);

    Task<(IEnumerable<Recipe> Items, bool HasMore)> GetFeedAsync(
        IEnumerable<int> followingUserIds,
        RecipeQueryParameters parameters);

    Task<(IEnumerable<Recipe> Items, bool HasMore)> GetExploreAsync(RecipeQueryParameters parameters);

    Task<(IEnumerable<Recipe> Items, bool HasMore)> GetFeaturedAsync(RecipeQueryParameters parameters);
    Task<int> GetCountByUserAsync(int userId);
    Task<IReadOnlyList<Recipe>> GetRecentByUserAsync(int userId, int take);
    Task<IEnumerable<Recipe>> GetAllPagedAsync(AdminRecipeListQuery query);
    Task<int> CountAllAsync(AdminRecipeListQuery query);
    Task<Recipe?> GetDetailedByIdForAdminAsync(int id);
    Task<int> CountRecipesAsync();
    Task<List<int>> GetMostPopularRecipeIdsAsync(int take);
}
