using RecipeShare.Application.DTOs.Users.Admin;

namespace RecipeShare.Application.Interfaces.Services;

public interface IUserStatsService
{
    Task ApplyStatsAsync(AdminUserDetailResponse response, int userId);
}
