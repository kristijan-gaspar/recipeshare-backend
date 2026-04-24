using Microsoft.EntityFrameworkCore;
using RecipeShare.Application.DTOs.Comments;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Domain.Entities;
using RecipeShare.Infrastructure.Data;

namespace RecipeShare.Infrastructure.Repositories;

internal class CommentRepository : GenericRepository<Comment>, ICommentRepository
{
    public CommentRepository(AppDbContext context) : base(context) { }

    public async Task<(IEnumerable<Comment> Items, bool HasMore)> GetCursorPagedByRecipeAsync(int recipeId, CommentQueryParameters parameters)
    {
        var query = _dbSet
            .Include(c => c.User)
            .Where(c => c.RecipeId == recipeId)
            .AsQueryable();

        if (parameters.Cursor.HasValue)
        {
            var anchor = await _dbSet.FindAsync(parameters.Cursor.Value);
            if (anchor != null)
                query = query.Where(c =>
                    c.CreatedAt < anchor.CreatedAt ||
                    (c.CreatedAt == anchor.CreatedAt && c.Id < anchor.Id));
        }

        query = query.OrderByDescending(c => c.CreatedAt).ThenByDescending(c => c.Id);

        var items = await query.Take(parameters.PageSize + 1).ToListAsync();
        var hasMore = items.Count > parameters.PageSize;

        return (items.Take(parameters.PageSize), hasMore);
    }

    public async Task<Comment?> GetByIdWithUserAsync(int id)
    {
        return await _dbSet
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<int> GetCountByRecipeAsync(int recipeId)
    {
        return await _dbSet.CountAsync(c => c.RecipeId == recipeId);
    }

    public async Task<Dictionary<int, int>> GetCountsByRecipeIdsAsync(IEnumerable<int> recipeIds)
    {
        return await _dbSet
            .Where(c => recipeIds.Contains(c.RecipeId))
            .GroupBy(c => c.RecipeId)
            .Select(g => new { RecipeId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.RecipeId, x => x.Count);
    }
}
