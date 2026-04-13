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

    public void Delete(RefreshToken token)
    {
        _context.RefreshTokens.Remove(token);
    }

    public async Task DeleteAllByUserIdAsync(int userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(r => r.UserId == userId)
            .ToListAsync();

        _context.RefreshTokens.RemoveRange(tokens);
    }
}
