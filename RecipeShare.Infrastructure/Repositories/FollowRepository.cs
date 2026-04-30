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

internal class FollowRepository : GenericRepository<Follow>, IFollowRepository
{
    public FollowRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Follow?> GetByUsersAsync(int followerId, int followedId)
    {
        return await _dbSet.
            FirstOrDefaultAsync(f => followerId == f.FollowerId && followedId == f.FollowedId);
    }

    public async Task<int> GetFollowerCountAsync(int userId)
    {
        return await _dbSet.CountAsync(f => f.FollowedId == userId);
    }

    public async Task<int> GetFollowingCountAsync(int userId)
    {
        return await _dbSet.CountAsync(f => f.FollowerId == userId);
    }

    public async Task<bool> ExistsAsync(int followerId, int followedId)
    {
        return await _dbSet
            .AnyAsync(f => f.FollowerId == followerId && f.FollowedId == followedId);
    }

    public async Task<List<int>> GetFollowingUserIdsAsync(int userId)
    {
        return await _context.Follows
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowedId)
            .ToListAsync();
    }
}
