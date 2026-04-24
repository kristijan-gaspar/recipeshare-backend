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
}
