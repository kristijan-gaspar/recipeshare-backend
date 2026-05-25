using Moq;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Services;

namespace RecipeShare.Application.UnitTests.Services;

public class AdminRecipeServiceTests
{
    private readonly Mock<IRecipeRepository> _recipeRepoMock = new();
    private readonly Mock<ICommentRepository> _commentRepoMock = new();
    private readonly Mock<ILikeRepository> _likeRepoMock = new();
    private readonly Mock<IRatingRepository> _ratingRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly AdminRecipeService _sut;

    public AdminRecipeServiceTests()
    {
        _sut = new AdminRecipeService(
            _recipeRepoMock.Object,
            _commentRepoMock.Object,
            _likeRepoMock.Object,
            _ratingRepoMock.Object,
            _unitOfWorkMock.Object);
    }
}
