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

internal class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context)
    {
    }

    public async  Task<List<Category>> GetAllAsync(string? searchTerm = null)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalizedTerm = searchTerm.Trim().ToLower();

            query = query.Where(c => c.Name.ToLower().Contains(normalizedTerm));
        }

        return await query
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<bool> NameExistsAsync(string name)
    {
        return await _dbSet.AnyAsync(c => c.Name.ToLower() == name.ToLower());
    }
}
