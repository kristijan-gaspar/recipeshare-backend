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
}
