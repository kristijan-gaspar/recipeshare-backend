using Moq;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Services;
using Xunit;

namespace RecipeShare.Application.UnitTests.Services;

public class AdminDashboardServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IRecipeRepository> _recipeRepoMock = new();

    private readonly AdminDashboardService _sut;

    public AdminDashboardServiceTests()
    {
        _sut = new AdminDashboardService(
            _userRepoMock.Object,
            _recipeRepoMock.Object);
    }

    [Fact]
    public async Task GetDashboardAsync_ReturnsAggregatedStats()
    {
        // Arrange
        _userRepoMock.Setup(r => r.CountUsersAsync()).ReturnsAsync(150);
        _recipeRepoMock.Setup(r => r.CountRecipesAsync()).ReturnsAsync(320);
        _userRepoMock.Setup(r => r.GetMostActiveUserIdsAsync(10)).ReturnsAsync(new List<int> { 1, 2, 3 });
        _recipeRepoMock.Setup(r => r.GetMostPopularRecipeIdsAsync(10)).ReturnsAsync(new List<int> { 10, 20, 30 });

        // Act
        var result = await _sut.GetDashboardAsync();

        // Assert
        Assert.Equal(150, result.TotalUsers);
        Assert.Equal(320, result.TotalRecipes);
        Assert.Equal(new List<int> { 1, 2, 3 }, result.MostActiveUsersIds);
        Assert.Equal(new List<int> { 10, 20, 30 }, result.MostPopularRecipesIds);
    }
}
