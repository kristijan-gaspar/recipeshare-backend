using RecipeShare.Application.DTOs.Dashboard;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;

namespace RecipeShare.Application.Services;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly IUserRepository _userRepository;
    private readonly IRecipeRepository _recipeRepository;

    public AdminDashboardService(
        IUserRepository userRepository,
        IRecipeRepository recipeRepository)
    {
        _userRepository = userRepository;
        _recipeRepository = recipeRepository;
    }

    public async Task<AdminDashboardResponse> GetDashboardAsync()
    {
        const int topCount = 10;

        var totalUsers = await _userRepository.CountUsersAsync();
        var totalRecipes = await _recipeRepository.CountRecipesAsync();
        var mostActiveUserIds = await _userRepository.GetMostActiveUserIdsAsync(topCount);
        var mostPopularRecipeIds = await _recipeRepository.GetMostPopularRecipeIdsAsync(topCount);

        return new AdminDashboardResponse
        {
            TotalUsers = totalUsers,
            TotalRecipes = totalRecipes,
            MostActiveUsersIds = mostActiveUserIds,
            MostPopularRecipesIds = mostPopularRecipeIds
        };
    }
}