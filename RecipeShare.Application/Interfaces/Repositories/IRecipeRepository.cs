using RecipeShare.Application.DTOs.Recipes;
using RecipeShare.Domain.Entities;

namespace RecipeShare.Application.Interfaces.Repositories;

public interface IRecipeRepository : IGenericRepository<Recipe>
{
    Task<(IEnumerable<Recipe> Items, bool HasMore)> GetCursorPagedAsync(RecipeQueryParameters parameters);
    Task<Recipe?> GetDetailedByIdAsync(int id);
    Task<bool> IsAuthorAsync(int recipeId, int userId);
    void RemoveIngredients(IEnumerable<Ingredient> ingredients);
    void RemoveSteps(IEnumerable<Step> steps);
}
