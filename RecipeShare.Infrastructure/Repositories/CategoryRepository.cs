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

    public async Task<List<Category>> GetAllAsync()
    {
        return await _dbSet.Include(c => c.Recipes).ToListAsync();
    }

    public async Task<Category?> GetByIdWithRecipesAsync(int id)
    {
        return await _dbSet.Include(c => c.Recipes).FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> NameExistsAsync(string name)
    {
        return await _dbSet.AnyAsync(c => c.Name.ToLower() == name.ToLower());
    }
}
