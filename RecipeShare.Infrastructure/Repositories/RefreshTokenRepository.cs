using Microsoft.EntityFrameworkCore;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Domain.Entities;
using RecipeShare.Infrastructure.Data;

namespace RecipeShare.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(RefreshToken token)
    {
        await _context.RefreshTokens.AddAsync(token);
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.TokenHash == tokenHash);
    }

    public async Task<List<RefreshToken>> GetActiveByUserIdAsync(int userId)
    {
        return await _context.RefreshTokens
            .Where(r => r.UserId == userId && !r.IsRevoked && r.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task RevokeAllByUserIdAsync(int userId)
    {
        var activeTokens = await GetActiveByUserIdAsync(userId);
        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
        }
    }
}
