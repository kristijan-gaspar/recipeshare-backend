using RecipeShare.Domain.Entities;

namespace RecipeShare.Application.Interfaces.Repositories;

public interface IRatingRepository : IGenericRepository<Rating>
{
    Task<Rating?> GetByUserAndRecipeAsync(int userId, int recipeId);
    Task<(double Avg, int Count)> GetStatsByRecipeAsync(int recipeId);
}
