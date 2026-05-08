using Microsoft.EntityFrameworkCore;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Domain.Entities;
using RecipeShare.Infrastructure.Data;

namespace RecipeShare.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted && !u.IsBlocked);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(u => u.Email == email);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _dbSet.AnyAsync(u => u.Username == username);
    }

    public async Task<List<User>> SearchByUsername(string query, int pageNumber, int pageSize)
    {
        var normalizedQuery = query.Trim().ToLower();

        return await _dbSet
            .Where(u => u.Username.Contains(normalizedQuery) && !u.IsDeleted && !u.IsBlocked)
            .OrderBy(u => u.Username)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountByUsernameAsync(string query)
    {
        var normalizedQuery = query.Trim().ToLower();

        return await _context.Users
            .CountAsync(u => u.Username.ToLower().Contains(normalizedQuery) && !u.IsDeleted && !u.IsBlocked);
    }

    public async Task<IEnumerable<User>> GetAllPagedAsync(string? query, int pageNumber, int pageSize)
    {
        var q = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(u => u.Username.Contains(query.Trim().ToLower()));

        return await q
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAllAsync(string? query)
    {
        var q = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(u => u.Username.Contains(query.Trim().ToLower()));

        return await q.CountAsync();
    }
}
