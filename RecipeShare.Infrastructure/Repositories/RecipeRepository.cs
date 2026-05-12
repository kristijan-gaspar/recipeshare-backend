using Microsoft.EntityFrameworkCore;
using RecipeShare.Application.DTOs.Recipes;
using RecipeShare.Application.DTOs.Recipes.Admin;
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
            .Where(r => !r.IsDeleted && !r.User.IsDeleted && !r.User.IsBlocked)
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
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted && !r.User.IsDeleted && !r.User.IsBlocked);
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


    private IQueryable<Recipe> WithSummaryIncludes()
    {
        return _dbSet
            .Where(r => !r.IsDeleted && !r.User.IsDeleted && !r.User.IsBlocked)
            .Include(r => r.User)
            .Include(r => r.Category)
            .Include(r => r.Tags);
    }

    private static IQueryable<Recipe> ApplyFilters(IQueryable<Recipe> query, RecipeQueryParameters parameters)
    {
        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim().ToLower();

            query = query.Where(r =>
                r.Title.ToLower().Contains(search) ||
                (r.Description != null && r.Description.ToLower().Contains(search)));
        }

        if (parameters.CategoryId.HasValue)
        {
            query = query.Where(r => r.CategoryId == parameters.CategoryId.Value);
        }

        if (parameters.TagIds != null && parameters.TagIds.Count > 0)
        {
            query = query.Where(r =>
                r.Tags.Any(t => parameters.TagIds.Contains(t.Id)));
        }

        if (parameters.Difficulty.HasValue)
        {
            query = query.Where(r => r.Difficulty == parameters.Difficulty.Value);
        }

        return query;
    }


    public async Task<(IEnumerable<Recipe> Items, bool HasMore)> GetFeedAsync(
        IEnumerable<int> followingUserIds,
        RecipeQueryParameters parameters)
    {
        var ids = followingUserIds.ToList();

        var query = WithSummaryIncludes()
            .Where(r => ids.Contains(r.UserId));

        query = ApplyFilters(query, parameters);

        if (parameters.Cursor.HasValue)
        {
            var anchor = await _dbSet.FindAsync(parameters.Cursor.Value);
            if (anchor != null)
                query = query.Where(r =>
                    r.CreatedAt < anchor.CreatedAt ||
                    (r.CreatedAt == anchor.CreatedAt && r.Id < anchor.Id));
        }

        query = query
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.Id);

        var items = await query.Take(parameters.PageSize + 1).ToListAsync();
        var hasMore = items.Count > parameters.PageSize;

        return (items.Take(parameters.PageSize), hasMore);
    }

    public async Task<(IEnumerable<Recipe> Items, bool HasMore)> GetFeaturedAsync(
        RecipeQueryParameters parameters)
    {
        var query = WithSummaryIncludes()
            .Where(r => r.IsFeatured);

        query = ApplyFilters(query, parameters);

        if (parameters.Cursor.HasValue)
        {
            var anchor = await _dbSet.FindAsync(parameters.Cursor.Value);
            if (anchor != null)
                query = query.Where(r =>
                    r.CreatedAt < anchor.CreatedAt ||
                    (r.CreatedAt == anchor.CreatedAt && r.Id < anchor.Id));
        }

        query = query
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.Id);

        var items = await query.Take(parameters.PageSize + 1).ToListAsync();
        var hasMore = items.Count > parameters.PageSize;

        return (items.Take(parameters.PageSize), hasMore);
    }

    public async Task<int> GetCountByUserAsync(int userId)
    {
        return await _dbSet.CountAsync(r => r.UserId == userId && !r.IsDeleted);
    }

    public async Task<IReadOnlyList<Recipe>> GetRecentByUserAsync(int userId, int take)
    {
        return await _dbSet
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<Recipe>> GetAllPagedAsync(AdminRecipeListQuery query)
    {
        var q = _dbSet
            .Include(r => r.User)
            .Include(r => r.Category)
            .Include(r => r.Tags)
            .AsNoTracking()
            .AsQueryable();

        q = ApplyAdminFilters(q, query);

        return await q
            .OrderByDescending(r => r.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();
    }

    public async Task<int> CountAllAsync(AdminRecipeListQuery query)
    {
        var q = _dbSet.AsNoTracking().AsQueryable();
        q = ApplyAdminFilters(q, query);
        return await q.CountAsync();
    }

    private static IQueryable<Recipe> ApplyAdminFilters(IQueryable<Recipe> q, AdminRecipeListQuery query)
    {
        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(r => r.Title.ToLower().Contains(query.Search.ToLower()));

        if (query.CategoryId.HasValue)
            q = q.Where(r => r.CategoryId == query.CategoryId.Value);

        if (query.IsDeleted.HasValue)
            q = q.Where(r => r.IsDeleted == query.IsDeleted.Value);

        if (query.IsFeatured.HasValue)
            q = q.Where(r => r.IsFeatured == query.IsFeatured.Value);

        return q;
    }

    public async Task<Recipe?> GetDetailedByIdForAdminAsync(int id)
    {
        return await _dbSet
            .Include(r => r.User)
            .Include(r => r.Category)
            .Include(r => r.Tags)
            .Include(r => r.Ingredients.OrderBy(i => i.Order))
            .Include(r => r.Steps.OrderBy(s => s.Order))
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<(IEnumerable<Recipe> Items, bool HasMore)> GetExploreAsync(RecipeQueryParameters parameters)
    {
        var query = WithSummaryIncludes();

        query = ApplyFilters(query, parameters);

        var baseQuery = query.Select(r => new
        {
            Recipe = r,
            LikeCount = r.Likes.Count,
            RatingCount = r.Ratings.Count,
            AverageRating = r.Ratings.Any() ? r.Ratings.Average(x => (double)x.Value) : 0
        });

        if (parameters.Cursor.HasValue)
        {
            var anchor = await _dbSet
                .Where(r => r.Id == parameters.Cursor.Value)
                .Select(r => new
                {
                    r.Id,
                    LikeCount = r.Likes.Count,
                    RatingCount = r.Ratings.Count,
                    AverageRating = r.Ratings.Any() ? r.Ratings.Average(x => (double)x.Value) : 0
                })
                .FirstOrDefaultAsync();

            if (anchor != null)
            {
                baseQuery = baseQuery.Where(x =>
                    x.LikeCount < anchor.LikeCount ||
                    (x.LikeCount == anchor.LikeCount &&
                     x.AverageRating < anchor.AverageRating) ||
                    (x.LikeCount == anchor.LikeCount &&
                     x.AverageRating == anchor.AverageRating &&
                     x.RatingCount < anchor.RatingCount) ||
                    (x.LikeCount == anchor.LikeCount &&
                     x.AverageRating == anchor.AverageRating &&
                     x.RatingCount == anchor.RatingCount &&
                     x.Recipe.Id < anchor.Id));
            }
        }

        var items = await baseQuery
            .OrderByDescending(x => x.LikeCount)
            .ThenByDescending(x => x.AverageRating)
            .ThenByDescending(x => x.RatingCount)
            .ThenByDescending(x => x.Recipe.Id)
            .Take(parameters.PageSize + 1)
            .Select(x => x.Recipe)
            .ToListAsync();

        var hasMore = items.Count > parameters.PageSize;

        return (items.Take(parameters.PageSize), hasMore);
    }

    public async Task<int> CountRecipesAsync()
    {
        return await _dbSet.CountAsync(r => !r.IsDeleted && !r.User.IsDeleted && !r.User.IsBlocked);
    }

    public async Task<List<int>> GetMostPopularRecipeIdsAsync(int take)
    {
        return await _dbSet
            .Where(r => !r.IsDeleted && !r.User.IsDeleted && !r.User.IsBlocked)            
            .OrderByDescending(r => r.Likes.Count)
            .ThenByDescending(r => r.Ratings.Any()
                ? r.Ratings.Average(x => (double)x.Value) : 0)
            .ThenByDescending(r => r.Ratings.Count)
            .ThenByDescending(r => r.Id)
            .Take(take)
            .Select(r => r.Id)
            .ToListAsync();
    }
}
