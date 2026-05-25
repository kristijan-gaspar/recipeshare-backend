using Moq;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Services;

namespace RecipeShare.Application.UnitTests.Services;

public class CollectionServiceTests
{
    private readonly Mock<ICollectionRepository> _collectionRepoMock = new();
    private readonly Mock<IRecipeRepository> _recipeRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly CollectionService _sut;

    public CollectionServiceTests()
    {
        _sut = new CollectionService(
            _collectionRepoMock.Object,
            _recipeRepoMock.Object,
            _unitOfWorkMock.Object);
    }
}
