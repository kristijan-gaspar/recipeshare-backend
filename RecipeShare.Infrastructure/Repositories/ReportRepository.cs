using Microsoft.EntityFrameworkCore;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Domain.Entities;
using RecipeShare.Domain.Enums;
using RecipeShare.Infrastructure.Data;

namespace RecipeShare.Infrastructure.Repositories;

public class ReportRepository : GenericRepository<Report>, IReportRepository
{
    public ReportRepository(AppDbContext context) : base(context) { }

    public async Task<List<Report>> GetFilteredAsync(ReportStatus? status, int page, int pageSize)
    {
        var query = _dbSet
            .Include(r => r.Reporter)
            .Include(r => r.ReportedUser)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountFilteredAsync(ReportStatus? status)
    {
        var query = _dbSet.AsQueryable();
        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);
        return await query.CountAsync();
    }

    public async Task<Report?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(r => r.Reporter)
            .Include(r => r.ReportedUser)
            .Include(r => r.ResolvedByAdmin)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<bool> ExistsAsync(int reporterId, ReportTargetType targetType, int targetId)
    {
        return await _dbSet.AnyAsync(r =>
            r.ReporterId == reporterId &&
            r.TargetType == targetType &&
            r.TargetId == targetId);
    }
}
