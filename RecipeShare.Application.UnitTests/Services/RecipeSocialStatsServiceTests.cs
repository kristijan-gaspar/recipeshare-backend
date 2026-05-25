using Moq;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Services;

namespace RecipeShare.Application.UnitTests.Services;

public class RecipeSocialStatsServiceTests
{
    private readonly Mock<ILikeRepository> _likeRepoMock = new();
    private readonly Mock<IRatingRepository> _ratingRepoMock = new();
    private readonly Mock<ICommentRepository> _commentRepoMock = new();

    private readonly RecipeSocialStatsService _sut;

    public RecipeSocialStatsServiceTests()
    {
        _sut = new RecipeSocialStatsService(
            _likeRepoMock.Object,
            _ratingRepoMock.Object,
            _commentRepoMock.Object);
    }
}
