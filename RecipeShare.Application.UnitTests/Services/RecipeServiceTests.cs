using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Application.Services;

namespace RecipeShare.Application.UnitTests.Services;

public class RecipeServiceTests
{
    private readonly Mock<IRecipeRepository> _recipeRepoMock = new();
    private readonly Mock<ICategoryRepository> _categoryRepoMock = new();
    private readonly Mock<ITagRepository> _tagRepoMock = new();
    private readonly Mock<IRecipeSocialStatsService> _socialStatsMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IImageStorageService> _imageStorageMock = new();

    private readonly RecipeService _sut;

    public RecipeServiceTests()
    {
        _sut = new RecipeService(
            _recipeRepoMock.Object,
            _categoryRepoMock.Object,
            _tagRepoMock.Object,
            _socialStatsMock.Object,
            _unitOfWorkMock.Object,
            _imageStorageMock.Object,
            new NullLogger<RecipeService>());
    }
}
