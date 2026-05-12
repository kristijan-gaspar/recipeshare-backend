using RecipeShare.Application.DTOs.Dashboard;

namespace RecipeShare.Application.Interfaces.Services;

public interface IAdminDashboardService
{
    Task<AdminDashboardResponse> GetDashboardAsync();
}
