using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Application.Services;

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
}
