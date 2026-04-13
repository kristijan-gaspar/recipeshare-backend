using Microsoft.EntityFrameworkCore;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Domain.Entities;
using RecipeShare.Infrastructure.Data;

namespace RecipeShare.Infrastructure.Repositories;
public class TagRepository : GenericRepository<Tag>, ITagRepository
{
    public TagRepository(AppDbContext context) : base(context) { }

    public async Task<List<Tag>> GetAllAsync(string? searchTerm = null)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var normalizedTerm = searchTerm.Trim().ToLower();

            query = query.Where(t => t.Name.Contains(normalizedTerm, StringComparison.CurrentCultureIgnoreCase));
        }

        return await query
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<bool> NameExistsAsync(string name)
    {
        return await _dbSet.AnyAsync(t => t.Name.ToLower() == name.ToLower());
    }
}