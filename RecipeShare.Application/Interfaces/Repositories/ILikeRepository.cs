using RecipeShare.Domain.Entities;

namespace RecipeShare.Application.Interfaces.Repositories;

public interface ILikeRepository : IGenericRepository<Like>
{
    Task<Like?> GetByUserAndRecipeAsync(int userId, int recipeId);
    Task<int> GetCountByRecipeAsync(int recipeId);
}
