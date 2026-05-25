using Moq;
using RecipeShare.Application.DTOs.Recipes;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Application.Mappings;
using RecipeShare.Application.Services;
using RecipeShare.Domain.Entities;
using Xunit;

namespace RecipeShare.Application.UnitTests.Services;

public class RecipeResponseMapperTests
{
    private readonly Mock<IRecipeSocialStatsService> _socialStatsMock = new();

    private readonly RecipeResponseMapper _sut;

    static RecipeResponseMapperTests() => MappingConfig.Configure();

    public RecipeResponseMapperTests()
    {
        _sut = new RecipeResponseMapper(
            _socialStatsMock.Object);
    }

    [Fact]
    public async Task ToCursorPagedAsync_WhenEmptyList_ReturnsEmptyPage()
    {
        // Arrange / Act
        var result = await _sut.ToCursorPagedAsync(new List<Recipe>(), false, 1);

        // Assert
        Assert.Empty(result.Items);
        Assert.False(result.HasMore);
        Assert.Null(result.NextCursor);
        _socialStatsMock.Verify(s => s.ApplyStatsAsync(It.IsAny<List<RecipeSummaryResponse>>(), 1), Times.Once);
    }

    [Fact]
    public async Task ToCursorPagedAsync_WhenHasMore_SetsNextCursorToLastRecipeId()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new() { Id = 1, User = new User { Id = 1, Username = "u" }, Category = new Category { Name = "Cat" }, Tags = new List<Tag>() },
            new() { Id = 2, User = new User { Id = 1, Username = "u" }, Category = new Category { Name = "Cat" }, Tags = new List<Tag>() }
        };

        // Act
        var result = await _sut.ToCursorPagedAsync(recipes, true, 1);

        // Assert
        Assert.Equal(2, result.Items.Count());
        Assert.True(result.HasMore);
        Assert.Equal(2, result.NextCursor);
    }

    [Fact]
    public async Task ToCursorPagedAsync_WhenNotHasMore_NextCursorIsNull()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new() { Id = 5, User = new User { Id = 1, Username = "u" }, Category = new Category { Name = "Cat" }, Tags = new List<Tag>() }
        };

        // Act
        var result = await _sut.ToCursorPagedAsync(recipes, false, 1);

        // Assert
        Assert.False(result.HasMore);
        Assert.Null(result.NextCursor);
    }

    [Fact]
    public async Task ToCursorPagedAsync_CallsApplyStatsWithCorrectUserId()
    {
        // Arrange / Act
        await _sut.ToCursorPagedAsync(new List<Recipe>(), false, 42);

        // Assert
        _socialStatsMock.Verify(s => s.ApplyStatsAsync(It.IsAny<List<RecipeSummaryResponse>>(), 42), Times.Once);
    }
}
