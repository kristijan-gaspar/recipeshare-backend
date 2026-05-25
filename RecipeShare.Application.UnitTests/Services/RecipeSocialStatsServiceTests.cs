using Moq;
using RecipeShare.Application.DTOs.Recipes;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Services;
using RecipeShare.Domain.Entities;
using Xunit;

namespace RecipeShare.Application.UnitTests.Services;

public class RecipeSocialStatsServiceTests
{
    private readonly Mock<ILikeRepository> _likeRepoMock = new();
    private readonly Mock<IRatingRepository> _ratingRepoMock = new();
    private readonly Mock<ICommentRepository> _commentRepoMock = new();

    private readonly RecipeSocialStatsService _sut;

    public RecipeSocialStatsServiceTests()
    {
        _sut = new RecipeSocialStatsService(
            _likeRepoMock.Object,
            _ratingRepoMock.Object,
            _commentRepoMock.Object);
    }

    [Fact]
    public async Task ApplyStatsAsync_ForList_PopulatesAllStatFields()
    {
        // Arrange
        var responses = new List<RecipeSummaryResponse>
        {
            new() { Id = 1 },
            new() { Id = 2 }
        };
        var ids = new List<int> { 1, 2 };

        _likeRepoMock.Setup(r => r.GetCountsByRecipeIdsAsync(ids))
            .ReturnsAsync(new Dictionary<int, int> { [1] = 10, [2] = 5 });
        _likeRepoMock.Setup(r => r.GetLikedRecipeIdsAsync(99, ids))
            .ReturnsAsync(new HashSet<int> { 1 });
        _ratingRepoMock.Setup(r => r.GetStatsByRecipeIdsAsync(ids))
            .ReturnsAsync(new Dictionary<int, (double Avg, int Count)> { [1] = (4.5, 20), [2] = (3.0, 8) });
        _ratingRepoMock.Setup(r => r.GetMyRatingsByRecipeIdsAsync(99, ids))
            .ReturnsAsync(new Dictionary<int, int> { [1] = 5 });
        _commentRepoMock.Setup(r => r.GetCountsByRecipeIdsAsync(ids))
            .ReturnsAsync(new Dictionary<int, int> { [1] = 3, [2] = 1 });

        // Act
        await _sut.ApplyStatsAsync(responses, 99);

        // Assert
        Assert.Equal(10, responses[0].LikeCount);
        Assert.True(responses[0].IsLikedByMe);
        Assert.Equal(4.5, responses[0].AverageRating);
        Assert.Equal(20, responses[0].RatingCount);
        Assert.Equal(5, responses[0].MyRating);
        Assert.Equal(3, responses[0].CommentCount);

        Assert.Equal(5, responses[1].LikeCount);
        Assert.False(responses[1].IsLikedByMe);
        Assert.Equal(3.0, responses[1].AverageRating);
        Assert.Equal(8, responses[1].RatingCount);
        Assert.Null(responses[1].MyRating);
        Assert.Equal(1, responses[1].CommentCount);
    }

    [Fact]
    public async Task ApplyStatsAsync_ForSingleRecipe_PopulatesAllStatFields()
    {
        // Arrange
        var response = new RecipeDetailResponse { Id = 1 };

        _likeRepoMock.Setup(r => r.GetCountByRecipeAsync(1)).ReturnsAsync(15);
        _likeRepoMock.Setup(r => r.GetByUserAndRecipeAsync(99, 1)).ReturnsAsync(new Like());
        _ratingRepoMock.Setup(r => r.GetStatsByRecipeAsync(1)).ReturnsAsync((4.2, 30));
        _ratingRepoMock.Setup(r => r.GetByUserAndRecipeAsync(99, 1)).ReturnsAsync(new Rating { Value = 4 });
        _commentRepoMock.Setup(r => r.GetCountByRecipeAsync(1)).ReturnsAsync(6);

        // Act
        await _sut.ApplyStatsAsync(response, 99);

        // Assert
        Assert.Equal(15, response.LikeCount);
        Assert.True(response.IsLikedByMe);
        Assert.Equal(4.2, response.AverageRating);
        Assert.Equal(30, response.RatingCount);
        Assert.Equal(4, response.MyRating);
        Assert.Equal(6, response.CommentCount);
    }

    [Fact]
    public async Task ApplyStatsAsync_ForSingleRecipe_WhenNotLiked_SetsIsLikedByMeToFalse()
    {
        // Arrange
        var response = new RecipeDetailResponse { Id = 1 };

        _likeRepoMock.Setup(r => r.GetCountByRecipeAsync(1)).ReturnsAsync(0);
        _likeRepoMock.Setup(r => r.GetByUserAndRecipeAsync(99, 1)).ReturnsAsync((Like?)null);
        _ratingRepoMock.Setup(r => r.GetStatsByRecipeAsync(1)).ReturnsAsync((0.0, 0));
        _ratingRepoMock.Setup(r => r.GetByUserAndRecipeAsync(99, 1)).ReturnsAsync((Rating?)null);
        _commentRepoMock.Setup(r => r.GetCountByRecipeAsync(1)).ReturnsAsync(0);

        // Act
        await _sut.ApplyStatsAsync(response, 99);

        // Assert
        Assert.False(response.IsLikedByMe);
        Assert.Null(response.MyRating);
    }
}
