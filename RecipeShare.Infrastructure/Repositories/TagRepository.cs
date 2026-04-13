using Microsoft.EntityFrameworkCore;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Domain.Entities;
using RecipeShare.Infrastructure.Data;

namespace RecipeShare.Infrastructure.Repositories;
public class TagRepository : GenericRepository<Tag>, ITagRepository
{
    public TagRepository(AppDbContext context) : base(context) { }

    public async Task<List<Tag>> GetAllAsync()
    {
        return await _dbSet.Include(t => t.Recipes).ToListAsync();
    }

    public async Task<Tag?> GetByIdWithRecipesAsync(int id)
    {
        return await _dbSet.Include(t => t.Recipes).FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<Tag>> GetByIdsAsync(IEnumerable<int> ids)
    {
        return await _dbSet.Where(t => ids.Contains(t.Id)).ToListAsync();
    }

    public async Task<bool> NameExistsAsync(string name)
    {
        return await _dbSet.AnyAsync(t => t.Name.ToLower() == name.ToLower());
    }
}