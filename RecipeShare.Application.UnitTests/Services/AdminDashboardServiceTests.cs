using Moq;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Services;

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
}
