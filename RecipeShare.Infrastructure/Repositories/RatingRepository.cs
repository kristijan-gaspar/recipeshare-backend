using Microsoft.EntityFrameworkCore;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Domain.Entities;
using RecipeShare.Infrastructure.Data;

namespace RecipeShare.Infrastructure.Repositories;

internal class RatingRepository : GenericRepository<Rating>, IRatingRepository
{
    public RatingRepository(AppDbContext context) : base(context) { }

    public async Task<Rating?> GetByUserAndRecipeAsync(int userId, int recipeId)
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.UserId == userId && r.RecipeId == recipeId);
    }

    public async Task<(double Avg, int Count)> GetStatsByRecipeAsync(int recipeId)
    {
        var ratings = await _dbSet.Where(r => r.RecipeId == recipeId).ToListAsync();
        var count = ratings.Count;
        var avg = count > 0 ? ratings.Average(r => r.Value) : 0;
        return (avg, count);
    }

    public async Task<Dictionary<int, (double Avg, int Count)>> GetStatsByRecipeIdsAsync(IEnumerable<int> recipeIds)
    {
        return await _dbSet
            .Where(r => recipeIds.Contains(r.RecipeId))
            .GroupBy(r => r.RecipeId)
            .Select(g => new { RecipeId = g.Key, Avg = g.Average(x => (double)x.Value), Count = g.Count() })
            .ToDictionaryAsync(x => x.RecipeId, x => (x.Avg, x.Count));
    }

    public async Task<Dictionary<int, int>> GetMyRatingsByRecipeIdsAsync(int userId, IEnumerable<int> recipeIds)
    {
        return await _dbSet
            .Where(r => r.UserId == userId && recipeIds.Contains(r.RecipeId))
            .ToDictionaryAsync(r => r.RecipeId, r => r.Value);
    }

    public async Task<int> GetCountByUserAsync(int userId)
    {
        return await _dbSet.CountAsync(r => r.UserId == userId && !r.Recipe.IsDeleted);
    }
}
