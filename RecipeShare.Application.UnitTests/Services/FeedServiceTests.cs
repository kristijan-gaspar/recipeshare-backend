using Moq;
using RecipeShare.Application.DTOs.Common;
using RecipeShare.Application.DTOs.Recipes;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Application.Services;
using RecipeShare.Domain.Entities;
using Xunit;

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

    private static CursorPagedResponse<RecipeSummaryResponse> EmptyPage() =>
        new() { Items = new List<RecipeSummaryResponse>(), HasMore = false };

    [Fact]
    public async Task GetFeedAsync_WhenFollowingUsers_CallsGetFeedAsync()
    {
        // Arrange
        var followingIds = new List<int> { 2, 3 };
        var recipes = new List<Recipe>();
        _followRepoMock.Setup(r => r.GetFollowingUserIdsAsync(1)).ReturnsAsync(followingIds);
        _recipeRepoMock.Setup(r => r.GetFeedAsync(followingIds, It.IsAny<RecipeQueryParameters>()))
            .ReturnsAsync((recipes.AsEnumerable(), false));
        _recipeResponseMapperMock.Setup(m => m.ToCursorPagedAsync(It.IsAny<IEnumerable<Recipe>>(), false, 1))
            .ReturnsAsync(EmptyPage());

        // Act
        var result = await _sut.GetFeedAsync(1, new RecipeQueryParameters());

        // Assert
        _recipeRepoMock.Verify(r => r.GetFeedAsync(followingIds, It.IsAny<RecipeQueryParameters>()), Times.Once);
        _recipeRepoMock.Verify(r => r.GetExploreAsync(It.IsAny<RecipeQueryParameters>()), Times.Never);
    }

    [Fact]
    public async Task GetFeedAsync_WhenNotFollowingAnyone_CallsGetExploreAsync()
    {
        // Arrange
        _followRepoMock.Setup(r => r.GetFollowingUserIdsAsync(1)).ReturnsAsync(new List<int>());
        _recipeRepoMock.Setup(r => r.GetExploreAsync(It.IsAny<RecipeQueryParameters>()))
            .ReturnsAsync((Enumerable.Empty<Recipe>(), false));
        _recipeResponseMapperMock.Setup(m => m.ToCursorPagedAsync(It.IsAny<IEnumerable<Recipe>>(), false, 1))
            .ReturnsAsync(EmptyPage());

        // Act
        await _sut.GetFeedAsync(1, new RecipeQueryParameters());

        // Assert
        _recipeRepoMock.Verify(r => r.GetExploreAsync(It.IsAny<RecipeQueryParameters>()), Times.Once);
        _recipeRepoMock.Verify(r => r.GetFeedAsync(It.IsAny<List<int>>(), It.IsAny<RecipeQueryParameters>()), Times.Never);
    }

    [Fact]
    public async Task GetExploreAsync_CallsGetExploreAsyncAndMapper()
    {
        // Arrange
        _recipeRepoMock.Setup(r => r.GetExploreAsync(It.IsAny<RecipeQueryParameters>()))
            .ReturnsAsync((Enumerable.Empty<Recipe>(), false));
        _recipeResponseMapperMock.Setup(m => m.ToCursorPagedAsync(It.IsAny<IEnumerable<Recipe>>(), false, 1))
            .ReturnsAsync(EmptyPage());

        // Act
        var result = await _sut.GetExploreAsync(1, new RecipeQueryParameters());

        // Assert
        _recipeRepoMock.Verify(r => r.GetExploreAsync(It.IsAny<RecipeQueryParameters>()), Times.Once);
        _recipeResponseMapperMock.Verify(m => m.ToCursorPagedAsync(It.IsAny<IEnumerable<Recipe>>(), false, 1), Times.Once);
    }

    [Fact]
    public async Task GetFeaturedAsync_CallsGetFeaturedAsyncAndMapper()
    {
        // Arrange
        _recipeRepoMock.Setup(r => r.GetFeaturedAsync(It.IsAny<RecipeQueryParameters>()))
            .ReturnsAsync((Enumerable.Empty<Recipe>(), false));
        _recipeResponseMapperMock.Setup(m => m.ToCursorPagedAsync(It.IsAny<IEnumerable<Recipe>>(), false, 1))
            .ReturnsAsync(EmptyPage());

        // Act
        var result = await _sut.GetFeaturedAsync(1, new RecipeQueryParameters());

        // Assert
        _recipeRepoMock.Verify(r => r.GetFeaturedAsync(It.IsAny<RecipeQueryParameters>()), Times.Once);
        _recipeResponseMapperMock.Verify(m => m.ToCursorPagedAsync(It.IsAny<IEnumerable<Recipe>>(), false, 1), Times.Once);
    }
}
