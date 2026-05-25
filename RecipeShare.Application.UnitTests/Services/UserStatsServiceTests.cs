using Moq;
using RecipeShare.Application.DTOs.Users.Admin;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Services;
using RecipeShare.Domain.Entities;
using Xunit;

namespace RecipeShare.Application.UnitTests.Services;

public class UserStatsServiceTests
{
    private readonly Mock<IRecipeRepository> _recipeRepoMock = new();
    private readonly Mock<ICommentRepository> _commentRepoMock = new();
    private readonly Mock<ILikeRepository> _likeRepoMock = new();
    private readonly Mock<IRatingRepository> _ratingRepoMock = new();
    private readonly Mock<IFollowRepository> _followRepoMock = new();

    private readonly UserStatsService _sut;

    public UserStatsServiceTests()
    {
        _sut = new UserStatsService(
            _recipeRepoMock.Object,
            _commentRepoMock.Object,
            _likeRepoMock.Object,
            _ratingRepoMock.Object,
            _followRepoMock.Object);
    }

    [Fact]
    public async Task ApplyStatsAsync_PopulatesAllCountFields()
    {
        // Arrange
        var response = new AdminUserDetailResponse();

        _recipeRepoMock.Setup(r => r.GetCountByUserAsync(1)).ReturnsAsync(12);
        _commentRepoMock.Setup(r => r.GetCountByUserAsync(1)).ReturnsAsync(34);
        _likeRepoMock.Setup(r => r.GetCountByUserAsync(1)).ReturnsAsync(56);
        _ratingRepoMock.Setup(r => r.GetCountByUserAsync(1)).ReturnsAsync(7);
        _followRepoMock.Setup(r => r.GetFollowerCountAsync(1)).ReturnsAsync(100);
        _followRepoMock.Setup(r => r.GetFollowingCountAsync(1)).ReturnsAsync(50);
        _recipeRepoMock.Setup(r => r.GetRecentByUserAsync(1, 5)).ReturnsAsync(new List<Recipe>());
        _commentRepoMock.Setup(r => r.GetRecentByUserAsync(1, 5)).ReturnsAsync(new List<Comment>());

        // Act
        await _sut.ApplyStatsAsync(response, 1);

        // Assert
        Assert.Equal(12, response.RecipeCount);
        Assert.Equal(34, response.CommentCount);
        Assert.Equal(56, response.LikeCount);
        Assert.Equal(7, response.RatingCount);
        Assert.Equal(100, response.FollowerCount);
        Assert.Equal(50, response.FollowingCount);
    }

    [Fact]
    public async Task ApplyStatsAsync_PopulatesRecentRecipesAndComments()
    {
        // Arrange
        var response = new AdminUserDetailResponse();
        var recentRecipes = new List<Recipe>
        {
            new() { Id = 1, Title = "Pasta", IsFeatured = true, IsDeleted = false }
        };
        var recentComments = new List<Comment>
        {
            new() { Id = 10, Content = "Great!", RecipeId = 5, Recipe = new Recipe { Id = 5, Title = "Pizza" }, IsDeleted = false }
        };

        _recipeRepoMock.Setup(r => r.GetCountByUserAsync(1)).ReturnsAsync(1);
        _commentRepoMock.Setup(r => r.GetCountByUserAsync(1)).ReturnsAsync(1);
        _likeRepoMock.Setup(r => r.GetCountByUserAsync(1)).ReturnsAsync(0);
        _ratingRepoMock.Setup(r => r.GetCountByUserAsync(1)).ReturnsAsync(0);
        _followRepoMock.Setup(r => r.GetFollowerCountAsync(1)).ReturnsAsync(0);
        _followRepoMock.Setup(r => r.GetFollowingCountAsync(1)).ReturnsAsync(0);
        _recipeRepoMock.Setup(r => r.GetRecentByUserAsync(1, 5)).ReturnsAsync(recentRecipes);
        _commentRepoMock.Setup(r => r.GetRecentByUserAsync(1, 5)).ReturnsAsync(recentComments);

        // Act
        await _sut.ApplyStatsAsync(response, 1);

        // Assert
        Assert.Single(response.RecentRecipes);
        Assert.Equal("Pasta", response.RecentRecipes[0].Title);
        Assert.True(response.RecentRecipes[0].IsFeatured);

        Assert.Single(response.RecentComments);
        Assert.Equal("Great!", response.RecentComments[0].Content);
        Assert.Equal("Pizza", response.RecentComments[0].RecipeTitle);
    }
}
