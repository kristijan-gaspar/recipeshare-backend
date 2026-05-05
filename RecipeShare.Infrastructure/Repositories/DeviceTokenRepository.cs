using Microsoft.EntityFrameworkCore;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Domain.Entities;
using RecipeShare.Infrastructure.Data;

namespace RecipeShare.Infrastructure.Repositories;

public class DeviceTokenRepository : GenericRepository<DeviceToken>, IDeviceTokenRepository
{
    public DeviceTokenRepository(AppDbContext context) : base(context) { }

    public Task<DeviceToken?> GetByTokenAsync(string token) =>
        _context.DeviceTokens.FirstOrDefaultAsync(d => d.Token == token);

    public Task<List<string>> GetTokensByUserIdAsync(int userId) =>
        _context.DeviceTokens
            .Where(d => d.UserId == userId)
            .Select(d => d.Token)
            .ToListAsync();

    public async Task DeleteByTokenAsync(string token)
    {
        var entity = await GetByTokenAsync(token);
        if (entity != null)
            Delete(entity);
    }

    public Task DeleteByUserIdAsync(int userId) =>
        _context.DeviceTokens
            .Where(d => d.UserId == userId)
            .ExecuteDeleteAsync();
}
