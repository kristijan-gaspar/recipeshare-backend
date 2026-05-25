using Moq;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Application.Services;
using RecipeShare.Domain.Enums;
using Xunit;

namespace RecipeShare.Application.UnitTests.Services;

public class NotificationServiceTests
{
    private readonly Mock<IDeviceTokenRepository> _deviceTokenRepoMock = new();
    private readonly Mock<IPushNotificationSender> _senderMock = new();

    private readonly NotificationService _sut;

    public NotificationServiceTests()
    {
        _sut = new NotificationService(
            _deviceTokenRepoMock.Object,
            _senderMock.Object);
    }

    [Fact]
    public async Task SendLikeNotificationAsync_WhenNoTokens_DoesNotCallSender()
    {
        // Arrange
        _deviceTokenRepoMock.Setup(r => r.GetTokensByUserIdAsync(1)).ReturnsAsync(new List<string>());

        // Act
        await _sut.SendLikeNotificationAsync(1, "actor", "Pizza", 10);

        // Assert
        _senderMock.Verify(
            s => s.SendAsync(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task SendLikeNotificationAsync_WithTokens_CallsSenderWithCorrectData()
    {
        // Arrange
        var tokens = new List<string> { "token1", "token2" };
        _deviceTokenRepoMock.Setup(r => r.GetTokensByUserIdAsync(1)).ReturnsAsync(tokens);

        // Act
        await _sut.SendLikeNotificationAsync(1, "actor", "Pizza", 10);

        // Assert
        _senderMock.Verify(
            s => s.SendAsync(
                tokens,
                "New like",
                It.Is<string>(body => body.Contains("actor") && body.Contains("Pizza")),
                It.Is<Dictionary<string, string>>(d => d["type"] == "like" && d["recipeId"] == "10")),
            Times.Once);
    }

    [Fact]
    public async Task SendCommentNotificationAsync_WhenNoTokens_DoesNotCallSender()
    {
        // Arrange
        _deviceTokenRepoMock.Setup(r => r.GetTokensByUserIdAsync(1)).ReturnsAsync(new List<string>());

        // Act
        await _sut.SendCommentNotificationAsync(1, "commenter", "Pasta", 5);

        // Assert
        _senderMock.Verify(
            s => s.SendAsync(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task SendCommentNotificationAsync_WithTokens_CallsSenderWithCorrectData()
    {
        // Arrange
        var tokens = new List<string> { "token1" };
        _deviceTokenRepoMock.Setup(r => r.GetTokensByUserIdAsync(2)).ReturnsAsync(tokens);

        // Act
        await _sut.SendCommentNotificationAsync(2, "commenter", "Pasta", 5);

        // Assert
        _senderMock.Verify(
            s => s.SendAsync(
                tokens,
                "New comment",
                It.Is<string>(body => body.Contains("commenter") && body.Contains("Pasta")),
                It.Is<Dictionary<string, string>>(d => d["type"] == "comment" && d["recipeId"] == "5")),
            Times.Once);
    }

    [Fact]
    public async Task SendFollowNotificationAsync_WhenNoTokens_DoesNotCallSender()
    {
        // Arrange
        _deviceTokenRepoMock.Setup(r => r.GetTokensByUserIdAsync(1)).ReturnsAsync(new List<string>());

        // Act
        await _sut.SendFollowNotificationAsync(1, "follower", 99);

        // Assert
        _senderMock.Verify(
            s => s.SendAsync(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task SendFollowNotificationAsync_WithTokens_CallsSenderWithCorrectData()
    {
        // Arrange
        var tokens = new List<string> { "token1" };
        _deviceTokenRepoMock.Setup(r => r.GetTokensByUserIdAsync(1)).ReturnsAsync(tokens);

        // Act
        await _sut.SendFollowNotificationAsync(1, "follower", 99);

        // Assert
        _senderMock.Verify(
            s => s.SendAsync(
                tokens,
                "New follower",
                It.Is<string>(body => body.Contains("follower")),
                It.Is<Dictionary<string, string>>(d => d["type"] == "follow" && d["followerId"] == "99")),
            Times.Once);
    }

    [Fact]
    public async Task SendWarningNotificationAsync_WhenNoTokens_DoesNotCallSender()
    {
        // Arrange
        _deviceTokenRepoMock.Setup(r => r.GetTokensByUserIdAsync(1)).ReturnsAsync(new List<string>());

        // Act
        await _sut.SendWarningNotificationAsync(1, ReportReason.Spam);

        // Assert
        _senderMock.Verify(
            s => s.SendAsync(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task SendWarningNotificationAsync_WithTokens_CallsSenderWithWarningTitle()
    {
        // Arrange
        var tokens = new List<string> { "token1" };
        _deviceTokenRepoMock.Setup(r => r.GetTokensByUserIdAsync(1)).ReturnsAsync(tokens);

        // Act
        await _sut.SendWarningNotificationAsync(1, ReportReason.Spam);

        // Assert
        _senderMock.Verify(
            s => s.SendAsync(
                tokens,
                "Warning from admin",
                It.IsAny<string>(),
                It.Is<Dictionary<string, string>>(d => d["type"] == "warning")),
            Times.Once);
    }
}
