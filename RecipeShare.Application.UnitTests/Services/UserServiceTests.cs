using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RecipeShare.Application.DTOs.Users;
using RecipeShare.Application.Exceptions;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Application.Services;
using RecipeShare.Domain.Entities;
using Xunit;

namespace RecipeShare.Application.UnitTests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IImageStorageService> _imageStorageMock = new();
    private readonly Mock<IPasswordHasher<User>> _passwordHasherMock = new();
    private readonly Mock<IFollowRepository> _followRepoMock = new();

    private readonly UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(
            _userRepoMock.Object,
            _refreshTokenRepoMock.Object,
            _unitOfWorkMock.Object,
            _imageStorageMock.Object,
            _passwordHasherMock.Object,
            new NullLogger<UserService>(),
            _followRepoMock.Object);
    }

    [Fact]
    public async Task GetProfileAsync_WhenUserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetProfileAsync(1, null));
    }

    [Fact]
    public async Task GetProfileAsync_WhenUserIsDeleted_ThrowsNotFoundException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new User { IsDeleted = true });

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetProfileAsync(1, null));
    }

    [Fact]
    public async Task GetProfileAsync_ReturnsProfileWithFollowCounts()
    {
        // Arrange
        var user = new User { Id = 1, Username = "alice", Bio = "Chef" };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _followRepoMock.Setup(r => r.GetFollowerCountAsync(1)).ReturnsAsync(100);
        _followRepoMock.Setup(r => r.GetFollowingCountAsync(1)).ReturnsAsync(50);

        // Act
        var result = await _sut.GetProfileAsync(1, null);

        // Assert
        Assert.Equal(1, result.Id);
        Assert.Equal("alice", result.Username);
        Assert.Equal("Chef", result.Bio);
        Assert.Equal(100, result.FollowerCount);
        Assert.Equal(50, result.FollowingCount);
        Assert.False(result.IsFollowedByCurrentUser);
    }

    [Fact]
    public async Task GetProfileAsync_WhenCurrentUserProvided_ChecksFollowStatus()
    {
        // Arrange
        var user = new User { Id = 1, Username = "alice" };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _followRepoMock.Setup(r => r.GetFollowerCountAsync(1)).ReturnsAsync(0);
        _followRepoMock.Setup(r => r.GetFollowingCountAsync(1)).ReturnsAsync(0);
        _followRepoMock.Setup(r => r.ExistsAsync(2, 1)).ReturnsAsync(true);

        // Act
        var result = await _sut.GetProfileAsync(1, 2);

        // Assert
        Assert.True(result.IsFollowedByCurrentUser);
    }

    [Fact]
    public async Task UpdateProfileAsync_WhenUserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.UpdateProfileAsync(1, new UpdateProfileRequest { Username = "new", Bio = null }));
    }

    [Fact]
    public async Task UpdateProfileAsync_WhenNewUsernameTaken_ThrowsBadRequestException()
    {
        // Arrange
        var user = new User { Id = 1, Username = "old" };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.UsernameExistsAsync("taken")).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.UpdateProfileAsync(1, new UpdateProfileRequest { Username = "taken", Bio = null }));
    }

    [Fact]
    public async Task UpdateProfileAsync_UpdatesUsernameAndBioAndSavesChanges()
    {
        // Arrange
        var user = new User { Id = 1, Username = "old", Bio = "old bio" };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.UsernameExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

        // Act
        await _sut.UpdateProfileAsync(1, new UpdateProfileRequest { Username = "NEW", Bio = "new bio" });

        // Assert
        Assert.Equal("new", user.Username);
        Assert.Equal("new bio", user.Bio);
        _userRepoMock.Verify(r => r.Update(user), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenUserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.ChangePasswordAsync(1, new ChangePasswordRequest { CurrentPassword = "old", NewPassword = "new" }));
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenCurrentPasswordWrong_ThrowsUnauthorizedException()
    {
        // Arrange
        var user = new User { Id = 1, PasswordHash = "hash" };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _passwordHasherMock
            .Setup(h => h.VerifyHashedPassword(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(PasswordVerificationResult.Failed);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.ChangePasswordAsync(1, new ChangePasswordRequest { CurrentPassword = "wrong", NewPassword = "new" }));
    }

    [Fact]
    public async Task ChangePasswordAsync_UpdatesHashAndDeletesRefreshTokens()
    {
        // Arrange
        var user = new User { Id = 1, PasswordHash = "old-hash" };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _passwordHasherMock
            .Setup(h => h.VerifyHashedPassword(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(PasswordVerificationResult.Success);
        _passwordHasherMock
            .Setup(h => h.HashPassword(It.IsAny<User>(), It.IsAny<string>()))
            .Returns("new-hash");

        // Act
        await _sut.ChangePasswordAsync(1, new ChangePasswordRequest { CurrentPassword = "old", NewPassword = "new" });

        // Assert
        Assert.Equal("new-hash", user.PasswordHash);
        _refreshTokenRepoMock.Verify(r => r.DeleteAllByUserIdAsync(1), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangeEmailAsync_WhenNewEmailSameAsCurrent_ReturnsEarly()
    {
        // Arrange
        var user = new User { Id = 1, Email = "same@test.com", PasswordHash = "hash" };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _passwordHasherMock
            .Setup(h => h.VerifyHashedPassword(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(PasswordVerificationResult.Success);

        // Act
        await _sut.ChangeEmailAsync(1, new ChangeEmailRequest { CurrentPassword = "pwd", NewEmail = "SAME@TEST.COM" });

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ChangeEmailAsync_WhenEmailAlreadyTaken_ThrowsBadRequestException()
    {
        // Arrange
        var user = new User { Id = 1, Email = "old@test.com", PasswordHash = "hash" };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _passwordHasherMock
            .Setup(h => h.VerifyHashedPassword(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(PasswordVerificationResult.Success);
        _userRepoMock.Setup(r => r.EmailExistsAsync("taken@test.com")).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.ChangeEmailAsync(1, new ChangeEmailRequest { CurrentPassword = "pwd", NewEmail = "taken@test.com" }));
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
    public async Task DeleteAccountAsync_WhenUserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.DeleteAccountAsync(1, new DeleteAccountRequest { Password = "pwd" }));
    }

    [Fact]
    public async Task DeleteAccountAsync_WhenPasswordWrong_ThrowsUnauthorizedException()
    {
        // Arrange
        var user = new User { Id = 1, PasswordHash = "hash" };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _passwordHasherMock
            .Setup(h => h.VerifyHashedPassword(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(PasswordVerificationResult.Failed);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.DeleteAccountAsync(1, new DeleteAccountRequest { Password = "wrong" }));
    }

    [Fact]
    public async Task DeleteAccountAsync_DeletesUserAndSavesChanges()
    {
        // Arrange
        var user = new User { Id = 1, PasswordHash = "hash" };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _passwordHasherMock
            .Setup(h => h.VerifyHashedPassword(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(PasswordVerificationResult.Success);

        // Act
        await _sut.DeleteAccountAsync(1, new DeleteAccountRequest { Password = "correct" });

        // Assert
        _userRepoMock.Verify(r => r.Delete(user), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileImageAsync_WhenInvalidExtension_ThrowsBadRequestException()
    {
        // Arrange
        using var image = new MemoryStream(new byte[100]);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.UpdateProfileImageAsync(1, image, "photo.bmp"));
    }

    [Fact]
    public async Task UpdateProfileImageAsync_WhenFileTooLarge_ThrowsBadRequestException()
    {
        // Arrange
        using var image = new MemoryStream(new byte[6 * 1024 * 1024]);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.UpdateProfileImageAsync(1, image, "photo.jpg"));
    }

    [Fact]
    public async Task DeleteProfileImageAsync_WhenNoProfileImage_ReturnsEarly()
    {
        // Arrange
        var user = new User { Id = 1, ProfileImagePublicId = null };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        // Act
        await _sut.DeleteProfileImageAsync(1);

        // Assert
        _imageStorageMock.Verify(s => s.DeleteAsync(It.IsAny<string>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
