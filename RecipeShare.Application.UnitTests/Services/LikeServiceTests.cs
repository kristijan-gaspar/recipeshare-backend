using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RecipeShare.Application.Exceptions;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Application.Services;
using RecipeShare.Domain.Entities;
using Xunit;

namespace RecipeShare.Application.UnitTests.Services;

public class LikeServiceTests
{
    private readonly Mock<ILikeRepository> _likeRepoMock = new();
    private readonly Mock<IRecipeRepository> _recipeRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<INotificationService> _notificationServiceMock = new();

    private readonly LikeService _sut;

    public LikeServiceTests()
    {
        _sut = new LikeService(
            _likeRepoMock.Object,
            _recipeRepoMock.Object,
            _unitOfWorkMock.Object,
            _notificationServiceMock.Object,
            new NullLogger<LikeService>());
    }

    [Fact]
    public async Task ToggleAsync_WhenRecipeNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Recipe?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.ToggleAsync(1, 1, "actor"));
    }

    [Fact]
    public async Task ToggleAsync_WhenRecipeIsDeleted_ThrowsNotFoundException()
    {
        // Arrange
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Recipe { IsDeleted = true });

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.ToggleAsync(1, 1, "actor"));
    }

    [Fact]
    public async Task ToggleAsync_WhenLikeExists_DeletesLikeAndReturnsUnliked()
    {
        // Arrange
        var recipe = new Recipe { Id = 1, UserId = 2 };
        var existing = new Like { UserId = 1, RecipeId = 1 };
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(recipe);
        _likeRepoMock.Setup(r => r.GetByUserAndRecipeAsync(1, 1)).ReturnsAsync(existing);
        _likeRepoMock.Setup(r => r.GetCountByRecipeAsync(1)).ReturnsAsync(5);

        // Act
        var result = await _sut.ToggleAsync(1, 1, "actor");

        // Assert
        Assert.False(result.IsLiked);
        Assert.Equal(5, result.LikeCount);
        _likeRepoMock.Verify(r => r.Delete(existing), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ToggleAsync_WhenLikeDoesNotExist_AddsLikeAndReturnsLiked()
    {
        // Arrange
        var recipe = new Recipe { Id = 1, UserId = 2 };
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(recipe);
        _likeRepoMock.Setup(r => r.GetByUserAndRecipeAsync(1, 1)).ReturnsAsync((Like?)null);
        _likeRepoMock.Setup(r => r.GetCountByRecipeAsync(1)).ReturnsAsync(10);

        // Act
        var result = await _sut.ToggleAsync(1, 1, "actor");

        // Assert
        Assert.True(result.IsLiked);
        Assert.Equal(10, result.LikeCount);
        _likeRepoMock.Verify(r => r.AddAsync(It.IsAny<Like>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ToggleAsync_WhenOwnRecipeLiked_DoesNotSendNotification()
    {
        // Arrange
        var recipe = new Recipe { Id = 1, UserId = 1 };
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(recipe);
        _likeRepoMock.Setup(r => r.GetByUserAndRecipeAsync(1, 1)).ReturnsAsync((Like?)null);
        _likeRepoMock.Setup(r => r.GetCountByRecipeAsync(1)).ReturnsAsync(0);

        // Act
        await _sut.ToggleAsync(1, 1, "actor");

        // Assert
        _notificationServiceMock.Verify(
            n => n.SendLikeNotificationAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task ToggleAsync_WhenDifferentUser_SendsLikeNotification()
    {
        // Arrange
        var recipe = new Recipe { Id = 1, UserId = 2, Title = "My Recipe" };
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(recipe);
        _likeRepoMock.Setup(r => r.GetByUserAndRecipeAsync(1, 1)).ReturnsAsync((Like?)null);
        _likeRepoMock.Setup(r => r.GetCountByRecipeAsync(1)).ReturnsAsync(1);

        // Act
        await _sut.ToggleAsync(1, 1, "actor");

        // Assert
        _notificationServiceMock.Verify(n => n.SendLikeNotificationAsync(2, "actor", "My Recipe", 1), Times.Once);
    }

    [Fact]
    public async Task ToggleAsync_WhenNotificationFails_DoesNotPropagate()
    {
        // Arrange
        var recipe = new Recipe { Id = 1, UserId = 2, Title = "My Recipe" };
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(recipe);
        _likeRepoMock.Setup(r => r.GetByUserAndRecipeAsync(1, 1)).ReturnsAsync((Like?)null);
        _likeRepoMock.Setup(r => r.GetCountByRecipeAsync(1)).ReturnsAsync(1);
        _notificationServiceMock
            .Setup(n => n.SendLikeNotificationAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception("push failed"));

        // Act
        var ex = await Record.ExceptionAsync(() => _sut.ToggleAsync(1, 1, "actor"));

        // Assert
        Assert.Null(ex);
    }
}
