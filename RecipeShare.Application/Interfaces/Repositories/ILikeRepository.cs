using RecipeShare.Domain.Entities;

namespace RecipeShare.Application.Interfaces.Repositories;

public interface ILikeRepository : IGenericRepository<Like>
{
    Task<Like?> GetByUserAndRecipeAsync(int userId, int recipeId);
    Task<int> GetCountByRecipeAsync(int recipeId);
    Task<Dictionary<int, int>> GetCountsByRecipeIdsAsync(IEnumerable<int> recipeIds);
    Task<HashSet<int>> GetLikedRecipeIdsAsync(int userId, IEnumerable<int> recipeIds);
    Task<int> GetCountByUserAsync(int userId);
}
