using Moq;
using RecipeShare.Application.DTOs.Notifications;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Services;
using RecipeShare.Domain.Entities;
using Xunit;

namespace RecipeShare.Application.UnitTests.Services;

public class DeviceTokenServiceTests
{
    private readonly Mock<IDeviceTokenRepository> _deviceTokenRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly DeviceTokenService _sut;

    public DeviceTokenServiceTests()
    {
        _sut = new DeviceTokenService(
            _deviceTokenRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WhenTokenExistsForSameUser_DoesNotSaveChanges()
    {
        // Arrange
        var existing = new DeviceToken { Token = "fcm-token", UserId = 1 };
        _deviceTokenRepoMock.Setup(r => r.GetByTokenAsync("fcm-token")).ReturnsAsync(existing);

        // Act
        await _sut.RegisterAsync(1, new RegisterDeviceTokenRequest { Token = "fcm-token" });

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WhenTokenExistsForDifferentUser_UpdatesUserIdAndSavesChanges()
    {
        // Arrange
        var existing = new DeviceToken { Token = "fcm-token", UserId = 99 };
        _deviceTokenRepoMock.Setup(r => r.GetByTokenAsync("fcm-token")).ReturnsAsync(existing);

        // Act
        await _sut.RegisterAsync(1, new RegisterDeviceTokenRequest { Token = "fcm-token" });

        // Assert
        Assert.Equal(1, existing.UserId);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WhenTokenDoesNotExist_AddsNewTokenAndSavesChanges()
    {
        // Arrange
        _deviceTokenRepoMock.Setup(r => r.GetByTokenAsync("new-token")).ReturnsAsync((DeviceToken?)null);

        // Act
        await _sut.RegisterAsync(1, new RegisterDeviceTokenRequest { Token = "new-token" });

        // Assert
        _deviceTokenRepoMock.Verify(
            r => r.AddAsync(It.Is<DeviceToken>(dt => dt.Token == "new-token" && dt.UserId == 1)),
            Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UnregisterAsync_DeletesTokenAndSavesChanges()
    {
        // Arrange / Act
        await _sut.UnregisterAsync("fcm-token");

        // Assert
        _deviceTokenRepoMock.Verify(r => r.DeleteByTokenAsync("fcm-token"), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
