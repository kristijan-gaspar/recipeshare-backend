using Microsoft.EntityFrameworkCore;
using RecipeShare.Application.DTOs.Recipes;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Domain.Entities;
using RecipeShare.Infrastructure.Data;

namespace RecipeShare.Infrastructure.Repositories;

public class RecipeRepository : GenericRepository<Recipe>, IRecipeRepository
{
    public RecipeRepository(AppDbContext context) : base(context) { }

    public async Task<(IEnumerable<Recipe> Items, bool HasMore)> GetCursorPagedAsync(RecipeQueryParameters parameters)
    {
        var query = _dbSet
            .Include(r => r.User)
            .Include(r => r.Category)
            .Include(r => r.Tags)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.Search))
            query = query.Where(r => r.Title.ToLower().Contains(parameters.Search.ToLower()));

        if (parameters.CategoryId.HasValue)
            query = query.Where(r => r.CategoryId == parameters.CategoryId.Value);

        if (parameters.TagIds != null && parameters.TagIds.Count > 0)
            query = query.Where(r => r.Tags.Any(t => parameters.TagIds.Contains(t.Id)));

        if (parameters.Difficulty.HasValue)
            query = query.Where(r => r.Difficulty == parameters.Difficulty.Value);

        if (parameters.Cursor.HasValue)
        {
            var anchor = await _dbSet.FindAsync(parameters.Cursor.Value);
            if (anchor != null)
                query = query.Where(r =>
                    r.CreatedAt < anchor.CreatedAt ||
                    (r.CreatedAt == anchor.CreatedAt && r.Id < anchor.Id));
        }

        query = query.OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id);

        var items = await query.Take(parameters.PageSize + 1).ToListAsync();
        var hasMore = items.Count > parameters.PageSize;

        return (items.Take(parameters.PageSize), hasMore);
    }

    public async Task<Recipe?> GetDetailedByIdAsync(int id)
    {
        return await _dbSet
            .Include(r => r.User)
            .Include(r => r.Category)
            .Include(r => r.Tags)
            .Include(r => r.Ingredients.OrderBy(i => i.Order))
            .Include(r => r.Steps.OrderBy(s => s.Order))
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<bool> IsAuthorAsync(int recipeId, int userId)
    {
        return await _dbSet.AnyAsync(r => r.Id == recipeId && r.UserId == userId);
    }

    public void RemoveIngredients(IEnumerable<Ingredient> ingredients)
    {
        _context.Set<Ingredient>().RemoveRange(ingredients);
    }

    public void RemoveSteps(IEnumerable<Step> steps)
    {
        _context.Set<Step>().RemoveRange(steps);
    }
}
