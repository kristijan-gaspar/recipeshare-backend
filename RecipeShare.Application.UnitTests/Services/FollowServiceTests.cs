using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Application.Services;

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
}
