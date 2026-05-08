using RecipeShare.Domain.Entities;
using RecipeShare.Domain.Enums;

namespace RecipeShare.Application.Interfaces.Repositories;

public interface IReportRepository : IGenericRepository<Report>
{
    Task<List<Report>> GetFilteredAsync(ReportStatus? status, int page, int pageSize);
    Task<int> CountFilteredAsync(ReportStatus? status);
    Task<Report?> GetByIdWithDetailsAsync(int id);
    Task<bool> ExistsAsync(int reporterId, ReportTargetType targetType, int targetId);
}
