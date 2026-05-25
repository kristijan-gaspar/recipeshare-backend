using Moq;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Application.Services;

namespace RecipeShare.Application.UnitTests.Services;

public class FeedServiceTests
{
    private readonly Mock<IRecipeRepository> _recipeRepoMock = new();
    private readonly Mock<IFollowRepository> _followRepoMock = new();
    private readonly Mock<IRecipeResponseMapper> _recipeResponseMapperMock = new();

    private readonly FeedService _sut;

    public FeedServiceTests()
    {
        _sut = new FeedService(
            _recipeRepoMock.Object,
            _followRepoMock.Object,
            _recipeResponseMapperMock.Object);
    }
}
