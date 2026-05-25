using Microsoft.AspNetCore.Identity;
using Moq;
using RecipeShare.Application.DTOs.Auth;
using RecipeShare.Application.Exceptions;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Application.Services;
using RecipeShare.Domain.Entities;
using Xunit;

namespace RecipeShare.Application.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ITokenProvider> _tokenProviderMock = new();
    private readonly Mock<IPasswordHasher<User>> _passwordHasherMock = new();

    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(
            _userRepoMock.Object,
            _refreshTokenRepoMock.Object,
            _unitOfWorkMock.Object,
            _tokenProviderMock.Object,
            _passwordHasherMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailExists_ThrowsBadRequestException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.EmailExistsAsync("test@test.com")).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.RegisterAsync(new RegisterRequest { Email = "test@test.com", Username = "user", Password = "pwd" }));
    }

    [Fact]
    public async Task RegisterAsync_WhenUsernameExists_ThrowsBadRequestException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userRepoMock.Setup(r => r.UsernameExistsAsync("user")).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.RegisterAsync(new RegisterRequest { Email = "new@test.com", Username = "user", Password = "pwd" }));
    }

    [Fact]
    public async Task RegisterAsync_CreatesUserAndReturnsTokens()
    {
        // Arrange
        _userRepoMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userRepoMock.Setup(r => r.UsernameExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _passwordHasherMock.Setup(h => h.HashPassword(It.IsAny<User>(), It.IsAny<string>())).Returns("hashed");
        _tokenProviderMock.Setup(t => t.GenerateJwtToken(It.IsAny<User>())).Returns("jwt-token");
        _tokenProviderMock.Setup(t => t.GenerateRefreshToken()).Returns(new TokenResult("raw-refresh", DateTime.UtcNow.AddDays(7)));

        // Act
        var result = await _sut.RegisterAsync(new RegisterRequest { Email = "New@Test.com", Username = "NewUser", Password = "pwd" });

        // Assert
        Assert.Equal("jwt-token", result.Token);
        Assert.Equal("raw-refresh", result.RefreshToken);
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }

    [Fact]
    public async Task RegisterAsync_NormalizesEmailAndUsername()
    {
        // Arrange
        _userRepoMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userRepoMock.Setup(r => r.UsernameExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _passwordHasherMock.Setup(h => h.HashPassword(It.IsAny<User>(), It.IsAny<string>())).Returns("hashed");
        _tokenProviderMock.Setup(t => t.GenerateJwtToken(It.IsAny<User>())).Returns("jwt");
        _tokenProviderMock.Setup(t => t.GenerateRefreshToken()).Returns(new TokenResult("rt", DateTime.UtcNow.AddDays(7)));

        // Act
        await _sut.RegisterAsync(new RegisterRequest { Email = "TEST@MAIL.COM", Username = "MYUSER", Password = "pwd" });

        // Assert
        _userRepoMock.Verify(r => r.AddAsync(It.Is<User>(u => u.Email == "test@mail.com" && u.Username == "myuser")), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WhenUserNotFound_ThrowsUnauthorizedException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.LoginAsync(new LoginRequest { Email = "nobody@test.com", Password = "pwd" }));
    }

    [Fact]
    public async Task LoginAsync_WhenUserIsDeleted_ThrowsUnauthorizedException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync(new User { IsDeleted = true });

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.LoginAsync(new LoginRequest { Email = "deleted@test.com", Password = "pwd" }));
    }

    [Fact]
    public async Task LoginAsync_WhenUserIsBlocked_ThrowsForbiddenException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync(new User { IsBlocked = true });

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _sut.LoginAsync(new LoginRequest { Email = "blocked@test.com", Password = "pwd" }));
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIsWrong_ThrowsUnauthorizedException()
    {
        // Arrange
        var user = new User { Email = "test@test.com", PasswordHash = "hash" };
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        _passwordHasherMock
            .Setup(h => h.VerifyHashedPassword(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(PasswordVerificationResult.Failed);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.LoginAsync(new LoginRequest { Email = "test@test.com", Password = "wrongpwd" }));
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsTokens()
    {
        // Arrange
        var user = new User { Email = "test@test.com", PasswordHash = "hash" };
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        _passwordHasherMock
            .Setup(h => h.VerifyHashedPassword(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(PasswordVerificationResult.Success);
        _tokenProviderMock.Setup(t => t.GenerateJwtToken(It.IsAny<User>())).Returns("jwt-token");
        _tokenProviderMock.Setup(t => t.GenerateRefreshToken()).Returns(new TokenResult("refresh-token", DateTime.UtcNow.AddDays(7)));

        // Act
        var result = await _sut.LoginAsync(new LoginRequest { Email = "test@test.com", Password = "correctpwd" });

        // Assert
        Assert.Equal("jwt-token", result.Token);
        Assert.Equal("refresh-token", result.RefreshToken);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_WhenTokenNotFound_ThrowsUnauthorizedException()
    {
        // Arrange
        _refreshTokenRepoMock.Setup(r => r.GetByTokenHashAsync(It.IsAny<string>())).ReturnsAsync((RefreshToken?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.RefreshAsync(new RefreshTokenRequest { RefreshToken = "invalid-token" }));
    }

    [Fact]
    public async Task RefreshAsync_WhenTokenExpired_DeletesTokenAndThrowsUnauthorizedException()
    {
        // Arrange
        var expired = new RefreshToken { ExpiresAt = DateTime.UtcNow.AddDays(-1) };
        _refreshTokenRepoMock.Setup(r => r.GetByTokenHashAsync(It.IsAny<string>())).ReturnsAsync(expired);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.RefreshAsync(new RefreshTokenRequest { RefreshToken = "expired" }));

        _refreshTokenRepoMock.Verify(r => r.Delete(expired), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_WhenUserIsDeleted_DeletesTokenAndThrowsUnauthorizedException()
    {
        // Arrange
        var token = new RefreshToken { UserId = 1, ExpiresAt = DateTime.UtcNow.AddDays(7) };
        _refreshTokenRepoMock.Setup(r => r.GetByTokenHashAsync(It.IsAny<string>())).ReturnsAsync(token);
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new User { IsDeleted = true });

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.RefreshAsync(new RefreshTokenRequest { RefreshToken = "some-token" }));

        _refreshTokenRepoMock.Verify(r => r.Delete(token), Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_WhenUserIsBlocked_DeletesTokenAndThrowsForbiddenException()
    {
        // Arrange
        var token = new RefreshToken { UserId = 1, ExpiresAt = DateTime.UtcNow.AddDays(7) };
        _refreshTokenRepoMock.Setup(r => r.GetByTokenHashAsync(It.IsAny<string>())).ReturnsAsync(token);
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new User { IsBlocked = true });

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _sut.RefreshAsync(new RefreshTokenRequest { RefreshToken = "some-token" }));

        _refreshTokenRepoMock.Verify(r => r.Delete(token), Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_WithValidToken_ReturnsNewTokensAndRotatesRefreshToken()
    {
        // Arrange
        var token = new RefreshToken { UserId = 1, ExpiresAt = DateTime.UtcNow.AddDays(7) };
        var user = new User { Id = 1 };
        _refreshTokenRepoMock.Setup(r => r.GetByTokenHashAsync(It.IsAny<string>())).ReturnsAsync(token);
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _tokenProviderMock.Setup(t => t.GenerateJwtToken(user)).Returns("new-jwt");
        _tokenProviderMock.Setup(t => t.GenerateRefreshToken()).Returns(new TokenResult("new-refresh", DateTime.UtcNow.AddDays(7)));

        // Act
        var result = await _sut.RefreshAsync(new RefreshTokenRequest { RefreshToken = "valid-token" });

        // Assert
        Assert.Equal("new-jwt", result.Token);
        Assert.Equal("new-refresh", result.RefreshToken);
        _refreshTokenRepoMock.Verify(r => r.Delete(token), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_WhenTokenNotFound_DoesNothing()
    {
        // Arrange
        _refreshTokenRepoMock.Setup(r => r.GetByTokenHashAsync(It.IsAny<string>())).ReturnsAsync((RefreshToken?)null);

        // Act
        await _sut.LogoutAsync("missing-token");

        // Assert
        _refreshTokenRepoMock.Verify(r => r.Delete(It.IsAny<RefreshToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_DeletesTokenAndSavesChanges()
    {
        // Arrange
        var token = new RefreshToken { Id = 1 };
        _refreshTokenRepoMock.Setup(r => r.GetByTokenHashAsync(It.IsAny<string>())).ReturnsAsync(token);

        // Act
        await _sut.LogoutAsync("valid-token");

        // Assert
        _refreshTokenRepoMock.Verify(r => r.Delete(token), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
