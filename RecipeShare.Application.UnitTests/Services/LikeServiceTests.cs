using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Application.Services;

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
}
