using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RecipeShare.Application.Exceptions;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Application.Services;
using RecipeShare.Domain.Entities;
using Xunit;

namespace RecipeShare.Application.UnitTests.Services;

public class AdminUserServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IImageStorageService> _imageStorageMock = new();
    private readonly Mock<IUserStatsService> _userStatsMock = new();

    private readonly AdminUserService _sut;

    public AdminUserServiceTests()
    {
        _sut = new AdminUserService(
            _userRepoMock.Object,
            _refreshTokenRepoMock.Object,
            _unitOfWorkMock.Object,
            _imageStorageMock.Object,
            _userStatsMock.Object,
            new NullLogger<AdminUserService>());
    }

    [Fact]
    public async Task GetUserAsync_WhenUserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetUserAsync(1));
    }

    [Fact]
    public async Task GetUserAsync_CallsApplyStatsAndReturnsResponse()
    {
        // Arrange
        var user = new User { Id = 1, Username = "admin_user" };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        // Act
        var result = await _sut.GetUserAsync(1);

        // Assert
        _userStatsMock.Verify(s => s.ApplyStatsAsync(It.IsAny<RecipeShare.Application.DTOs.Users.Admin.AdminUserDetailResponse>(), 1), Times.Once);
    }

    [Fact]
    public async Task ToggleBlockAsync_WhenUserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.ToggleBlockAsync(1));
    }

    [Fact]
    public async Task ToggleBlockAsync_WhenUserIsDeleted_ThrowsBadRequestException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new User { IsDeleted = true });

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _sut.ToggleBlockAsync(1));
    }

    [Fact]
    public async Task ToggleBlockAsync_WhenBlocking_DeletesRefreshTokensAndSavesChanges()
    {
        // Arrange
        var user = new User { Id = 1, IsBlocked = false };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        // Act
        await _sut.ToggleBlockAsync(1);

        // Assert
        Assert.True(user.IsBlocked);
        _refreshTokenRepoMock.Verify(r => r.DeleteAllByUserIdAsync(1), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ToggleBlockAsync_WhenUnblocking_DoesNotDeleteRefreshTokens()
    {
        // Arrange
        var user = new User { Id = 1, IsBlocked = true };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        // Act
        await _sut.ToggleBlockAsync(1);

        // Assert
        Assert.False(user.IsBlocked);
        _refreshTokenRepoMock.Verify(r => r.DeleteAllByUserIdAsync(It.IsAny<int>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BlockAsync_WhenUserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.BlockAsync(1));
    }

    [Fact]
    public async Task BlockAsync_WhenUserIsDeleted_ThrowsBadRequestException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new User { IsDeleted = true });

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _sut.BlockAsync(1));
    }

    [Fact]
    public async Task BlockAsync_WhenAlreadyBlocked_DoesNothing()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new User { IsBlocked = true });

        // Act
        await _sut.BlockAsync(1);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BlockAsync_BlocksUserAndDeletesRefreshTokens()
    {
        // Arrange
        var user = new User { Id = 1, IsBlocked = false };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        // Act
        await _sut.BlockAsync(1);

        // Assert
        Assert.True(user.IsBlocked);
        _refreshTokenRepoMock.Verify(r => r.DeleteAllByUserIdAsync(1), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RestoreAsync_WhenUserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.RestoreAsync(1));
    }

    [Fact]
    public async Task RestoreAsync_WhenUserNotDeleted_ThrowsBadRequestException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new User { IsDeleted = false });

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _sut.RestoreAsync(1));
    }

    [Fact]
    public async Task RestoreAsync_ClearsIsDeletedAndSavesChanges()
    {
        // Arrange
        var user = new User { Id = 1, IsDeleted = true, DeletedAt = DateTime.UtcNow.AddDays(-1) };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        // Act
        await _sut.RestoreAsync(1);

        // Assert
        Assert.False(user.IsDeleted);
        Assert.Null(user.DeletedAt);
        _userRepoMock.Verify(r => r.Update(user), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SoftDeleteAsync_WhenUserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.SoftDeleteAsync(1));
    }

    [Fact]
    public async Task SoftDeleteAsync_WhenUserAlreadyDeleted_ThrowsNotFoundException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new User { IsDeleted = true });

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.SoftDeleteAsync(1));
    }

    [Fact]
    public async Task SoftDeleteAsync_SetsIsDeletedAndDeletesRefreshTokens()
    {
        // Arrange
        var user = new User { Id = 1 };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        // Act
        await _sut.SoftDeleteAsync(1);

        // Assert
        Assert.True(user.IsDeleted);
        Assert.NotNull(user.DeletedAt);
        _refreshTokenRepoMock.Verify(r => r.DeleteAllByUserIdAsync(1), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SoftDeleteAsync_WhenProfileImageExists_DeletesImage()
    {
        // Arrange
        var user = new User { Id = 1, ProfileImagePublicId = "public-id-123" };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        // Act
        await _sut.SoftDeleteAsync(1);

        // Assert
        _imageStorageMock.Verify(s => s.DeleteAsync("public-id-123", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SoftDeleteAsync_WhenImageDeletionFails_DoesNotPropagate()
    {
        // Arrange
        var user = new User { Id = 1, ProfileImagePublicId = "public-id-123" };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _imageStorageMock.Setup(s => s.DeleteAsync("public-id-123", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RecipeShare.Application.Exceptions.ImageStorageException("storage error"));

        // Act
        var ex = await Record.ExceptionAsync(() => _sut.SoftDeleteAsync(1));

        // Assert
        Assert.Null(ex);
    }
}
