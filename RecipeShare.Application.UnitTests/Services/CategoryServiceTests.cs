using Moq;
using RecipeShare.Application.DTOs.Categories;
using RecipeShare.Application.Exceptions;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Services;
using RecipeShare.Domain.Entities;
using Xunit;

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

    [Fact]
    public async Task GetCategoriesAsync_ReturnsOnlyActiveCategories()
    {
        // Arrange
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Active", IsActive = true, Recipes = new List<Recipe>() },
            new() { Id = 2, Name = "Inactive", IsActive = false, Recipes = new List<Recipe>() }
        };
        _categoryRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);

        // Act
        var result = await _sut.GetCategoriesAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Active", result[0].Name);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_WhenNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _categoryRepoMock.Setup(r => r.GetByIdWithRecipesAsync(1)).ReturnsAsync((Category?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetCategoryByIdAsync(1));
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ReturnsCorrectResponse()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Desserts", Recipes = new List<Recipe> { new(), new() } };
        _categoryRepoMock.Setup(r => r.GetByIdWithRecipesAsync(1)).ReturnsAsync(category);

        // Act
        var result = await _sut.GetCategoryByIdAsync(1);

        // Assert
        Assert.Equal(1, result.Id);
        Assert.Equal("Desserts", result.Name);
        Assert.Equal(2, result.RecipeCount);
    }

    [Fact]
    public async Task GetAdminCategoriesAsync_ReturnsAllCategories()
    {
        // Arrange
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "A", IsActive = true, Recipes = new List<Recipe>() },
            new() { Id = 2, Name = "B", IsActive = false, Recipes = new List<Recipe>() }
        };
        _categoryRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);

        // Act
        var result = await _sut.GetAdminCategoriesAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAdminCategoryByIdAsync_WhenNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _categoryRepoMock.Setup(r => r.GetByIdWithRecipesAsync(99)).ReturnsAsync((Category?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetAdminCategoryByIdAsync(99));
    }

    [Fact]
    public async Task GetAdminCategoryByIdAsync_ReturnsCorrectResponse()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Soups", IsActive = true, Recipes = new List<Recipe>() };
        _categoryRepoMock.Setup(r => r.GetByIdWithRecipesAsync(1)).ReturnsAsync(category);

        // Act
        var result = await _sut.GetAdminCategoryByIdAsync(1);

        // Assert
        Assert.Equal(1, result.Id);
        Assert.Equal("Soups", result.Name);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task CreateCategoryAsync_WhenNameExists_ThrowsBadRequestException()
    {
        // Arrange
        _categoryRepoMock.Setup(r => r.NameExistsAsync("Soups")).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.CreateCategoryAsync(new CreateCategoryRequest { Name = "Soups" }));
    }

    [Fact]
    public async Task CreateCategoryAsync_CreatesCategory_AndSavesChanges()
    {
        // Arrange
        _categoryRepoMock.Setup(r => r.NameExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

        // Act
        await _sut.CreateCategoryAsync(new CreateCategoryRequest { Name = "  Salads  " });

        // Assert
        _categoryRepoMock.Verify(r => r.AddAsync(It.Is<Category>(c => c.Name == "Salads")), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCategoryAsync_WhenNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _categoryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Category?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.UpdateCategoryAsync(1, new CreateCategoryRequest { Name = "New" }));
    }

    [Fact]
    public async Task UpdateCategoryAsync_WhenNameExistsForDifferentCategory_ThrowsBadRequestException()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "OldName" };
        _categoryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);
        _categoryRepoMock.Setup(r => r.NameExistsAsync("NewName")).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.UpdateCategoryAsync(1, new CreateCategoryRequest { Name = "NewName" }));
    }

    [Fact]
    public async Task UpdateCategoryAsync_WhenSameNameCaseInsensitive_DoesNotThrow()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Soups" };
        _categoryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);
        _categoryRepoMock.Setup(r => r.NameExistsAsync("soups")).ReturnsAsync(true);

        // Act
        var ex = await Record.ExceptionAsync(() =>
            _sut.UpdateCategoryAsync(1, new CreateCategoryRequest { Name = "soups" }));

        // Assert
        Assert.Null(ex);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCategoryAsync_UpdatesCategory_AndSavesChanges()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Old" };
        _categoryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);
        _categoryRepoMock.Setup(r => r.NameExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

        // Act
        await _sut.UpdateCategoryAsync(1, new CreateCategoryRequest { Name = "  New  " });

        // Assert
        _categoryRepoMock.Verify(r => r.Update(It.Is<Category>(c => c.Name == "New")), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCategoryAsync_WhenNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _categoryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Category?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteCategoryAsync(1));
    }

    [Fact]
    public async Task DeleteCategoryAsync_DeletesCategory_AndSavesChanges()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Soups" };
        _categoryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);

        // Act
        await _sut.DeleteCategoryAsync(1);

        // Assert
        _categoryRepoMock.Verify(r => r.Delete(category), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ToggleActiveAsync_WhenNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _categoryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Category?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.ToggleActiveAsync(1));
    }

    [Fact]
    public async Task ToggleActiveAsync_TogglesIsActive_AndSavesChanges()
    {
        // Arrange
        var category = new Category { Id = 1, IsActive = true };
        _categoryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);

        // Act
        await _sut.ToggleActiveAsync(1);

        // Assert
        Assert.False(category.IsActive);
        _categoryRepoMock.Verify(r => r.Update(category), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
