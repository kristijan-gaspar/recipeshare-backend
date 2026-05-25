using Moq;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Services;

namespace RecipeShare.Application.UnitTests.Services;

public class RatingServiceTests
{
    private readonly Mock<IRatingRepository> _ratingRepoMock = new();
    private readonly Mock<IRecipeRepository> _recipeRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly RatingService _sut;

    public RatingServiceTests()
    {
        _sut = new RatingService(
            _ratingRepoMock.Object,
            _recipeRepoMock.Object,
            _unitOfWorkMock.Object);
    }
}
