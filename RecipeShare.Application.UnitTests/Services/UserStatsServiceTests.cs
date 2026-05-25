using Moq;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Services;

namespace RecipeShare.Application.UnitTests.Services;

public class UserStatsServiceTests
{
    private readonly Mock<IRecipeRepository> _recipeRepoMock = new();
    private readonly Mock<ICommentRepository> _commentRepoMock = new();
    private readonly Mock<ILikeRepository> _likeRepoMock = new();
    private readonly Mock<IRatingRepository> _ratingRepoMock = new();
    private readonly Mock<IFollowRepository> _followRepoMock = new();

    private readonly UserStatsService _sut;

    public UserStatsServiceTests()
    {
        _sut = new UserStatsService(
            _recipeRepoMock.Object,
            _commentRepoMock.Object,
            _likeRepoMock.Object,
            _ratingRepoMock.Object,
            _followRepoMock.Object);
    }
}
