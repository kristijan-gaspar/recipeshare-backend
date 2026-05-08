using RecipeShare.Application.Common;
using RecipeShare.Application.DTOs.Reports;
using RecipeShare.Domain.Enums;

namespace RecipeShare.Application.Interfaces.Services;

public interface IReportService
{
    Task CreateAsync(int reporterId, CreateReportRequest request);
    Task<PagedResponse<ReportResponse>> GetReportsAsync(ReportStatus? status, int page, int pageSize);
    Task<ReportDetailResponse> GetByIdAsync(int reportId);
    Task ResolveAsync(int reportId, int adminId, ResolveReportRequest request);
    Task DismissAsync(int reportId, int adminId);
}
