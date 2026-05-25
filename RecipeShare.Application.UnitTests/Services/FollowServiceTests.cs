using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RecipeShare.Application.Exceptions;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Application.Services;
using RecipeShare.Domain.Entities;
using Xunit;

namespace RecipeShare.Application.UnitTests.Services;

public class FollowServiceTests
{
    private readonly Mock<IFollowRepository> _followRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<INotificationService> _notificationServiceMock = new();

    private readonly FollowService _sut;

    public FollowServiceTests()
    {
        _sut = new FollowService(
            _followRepoMock.Object,
            _userRepoMock.Object,
            _unitOfWorkMock.Object,
            _notificationServiceMock.Object,
            new NullLogger<FollowService>());
    }

    [Fact]
    public async Task ToggleFollowAsync_WhenSelfFollow_ThrowsBadRequestException()
    {
        // Arrange / Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.ToggleFollowAsync(5, 5, "user5"));
    }

    [Fact]
    public async Task ToggleFollowAsync_WhenTargetUserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.ToggleFollowAsync(2, 1, "user1"));
    }

    [Fact]
    public async Task ToggleFollowAsync_WhenTargetUserIsDeleted_ThrowsNotFoundException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new User { IsDeleted = true });

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.ToggleFollowAsync(2, 1, "user1"));
    }

    [Fact]
    public async Task ToggleFollowAsync_WhenAlreadyFollowing_DeletesFollowAndReturnsFalse()
    {
        // Arrange
        var targetUser = new User { Id = 2 };
        var existingFollow = new Follow { FollowerId = 1, FollowedId = 2 };
        _userRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(targetUser);
        _followRepoMock.Setup(r => r.GetByUsersAsync(1, 2)).ReturnsAsync(existingFollow);

        // Act
        var result = await _sut.ToggleFollowAsync(2, 1, "user1");

        // Assert
        Assert.False(result);
        _followRepoMock.Verify(r => r.Delete(existingFollow), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ToggleFollowAsync_WhenNotFollowing_AddsFollowAndReturnsTrue()
    {
        // Arrange
        var targetUser = new User { Id = 2 };
        _userRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(targetUser);
        _followRepoMock.Setup(r => r.GetByUsersAsync(1, 2)).ReturnsAsync((Follow?)null);

        // Act
        var result = await _sut.ToggleFollowAsync(2, 1, "user1");

        // Assert
        Assert.True(result);
        _followRepoMock.Verify(r => r.AddAsync(It.Is<Follow>(f => f.FollowerId == 1 && f.FollowedId == 2)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ToggleFollowAsync_WhenFollow_SendsFollowNotification()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new User { Id = 2 });
        _followRepoMock.Setup(r => r.GetByUsersAsync(1, 2)).ReturnsAsync((Follow?)null);

        // Act
        await _sut.ToggleFollowAsync(2, 1, "user1");

        // Assert
        _notificationServiceMock.Verify(n => n.SendFollowNotificationAsync(2, "user1", 1), Times.Once);
    }

    [Fact]
    public async Task ToggleFollowAsync_WhenNotificationFails_DoesNotPropagate()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new User { Id = 2 });
        _followRepoMock.Setup(r => r.GetByUsersAsync(1, 2)).ReturnsAsync((Follow?)null);
        _notificationServiceMock
            .Setup(n => n.SendFollowNotificationAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception("push failed"));

        // Act
        var ex = await Record.ExceptionAsync(() => _sut.ToggleFollowAsync(2, 1, "user1"));

        // Assert
        Assert.Null(ex);
    }
}
