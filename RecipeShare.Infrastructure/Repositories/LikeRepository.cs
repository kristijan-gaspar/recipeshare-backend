using Microsoft.EntityFrameworkCore;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Domain.Entities;
using RecipeShare.Infrastructure.Data;

namespace RecipeShare.Infrastructure.Repositories;

internal class LikeRepository : GenericRepository<Like>, ILikeRepository
{
    public LikeRepository(AppDbContext context) : base(context) { }

    public async Task<Like?> GetByUserAndRecipeAsync(int userId, int recipeId)
    {
        return await _dbSet.FirstOrDefaultAsync(l => l.UserId == userId && l.RecipeId == recipeId);
    }

    public async Task<int> GetCountByRecipeAsync(int recipeId)
    {
        return await _dbSet.CountAsync(l => l.RecipeId == recipeId);
    }

    public async Task<Dictionary<int, int>> GetCountsByRecipeIdsAsync(IEnumerable<int> recipeIds)
    {
        return await _dbSet
            .Where(l => recipeIds.Contains(l.RecipeId))
            .GroupBy(l => l.RecipeId)
            .Select(g => new { RecipeId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.RecipeId, x => x.Count);
    }

    public async Task<HashSet<int>> GetLikedRecipeIdsAsync(int userId, IEnumerable<int> recipeIds)
    {
        var liked = await _dbSet
            .Where(l => l.UserId == userId && recipeIds.Contains(l.RecipeId))
            .Select(l => l.RecipeId)
            .ToListAsync();

        return liked.ToHashSet();
    }

    public async Task<int> GetCountByUserAsync(int userId)
    {
        return await _dbSet.CountAsync(l => l.UserId == userId && !l.Recipe.IsDeleted);
    }
}
