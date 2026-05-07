using RecipeShare.Application.Common;
using RecipeShare.Application.DTOs.Users.Admin;

namespace RecipeShare.Application.Interfaces.Services;

public interface IAdminUserService
{
    Task<PagedResponse<AdminUserListItemResponse>> GetUsersAsync(AdminUserListQuery query);
    Task<AdminUserDetailResponse> GetUserAsync(int userId);
    Task ToggleBlockAsync(int userId, bool isBlocked);
    Task SoftDeleteAsync(int userId);
}
