using Microsoft.EntityFrameworkCore;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Domain.Entities;
using RecipeShare.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShare.Infrastructure.Repositories;

public class CollectionRepository : GenericRepository<Collection>, ICollectionRepository
{
    public CollectionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Collection?> GetByIdAndUserIdAsync(int collectionId, int userId)
    {
        return await _context.Collections
            .Include(c => c.CollectionRecipes)
            .FirstOrDefaultAsync(c => c.Id == collectionId && c.UserId == userId);
    }

    public async Task<List<Collection>> GetByUserIdAsync(int userId)
    {
        return await _context.Collections
            .Where(c => c.UserId == userId)
            .Include(c=> c.CollectionRecipes)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Collection?> GetWithRecipesAsync(int collectionId)
    {
        return await _context.Collections
            .Include(c => c.CollectionRecipes)
                .ThenInclude(cr => cr.Recipe)
                    .ThenInclude(r => r.User)
            .Include(c => c.CollectionRecipes)
                .ThenInclude(cr => cr.Recipe)
                    .ThenInclude(r => r.Category)
            .Include(c => c.CollectionRecipes)
                .ThenInclude(cr => cr.Recipe)
                    .ThenInclude(r => r.Tags)
            .FirstOrDefaultAsync(c => c.Id == collectionId);
    }

    public async Task<bool> NameExistsForUserAsync(int userId, string name)
    {
        return await _context.Collections
            .AnyAsync(c => c.UserId == userId && c.Name.ToLower() == name.ToLower());
    }
}
