using Moq;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Services;

namespace RecipeShare.Application.UnitTests.Services;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly CategoryService _sut;

    public CategoryServiceTests()
    {
        _sut = new CategoryService(
            _categoryRepoMock.Object,
            _unitOfWorkMock.Object);
    }
}
