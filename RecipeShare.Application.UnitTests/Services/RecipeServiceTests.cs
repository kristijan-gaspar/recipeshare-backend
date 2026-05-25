using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RecipeShare.Application.DTOs.Recipes;
using RecipeShare.Application.Exceptions;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Application.Mappings;
using RecipeShare.Application.Services;
using RecipeShare.Domain.Entities;
using Xunit;

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

    static RecipeServiceTests() => MappingConfig.Configure();

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

    private static CreateRecipeRequest ValidCreateRequest(int categoryId = 1, List<int>? tagIds = null) =>
        new()
        {
            Title = "Test Recipe",
            CategoryId = categoryId,
            TagIds = tagIds ?? new List<int>(),
            Ingredients = new List<IngredientRequest> { new() { Name = "Salt", Quantity = "1", Order = 1 } },
            Steps = new List<StepRequest> { new() { Description = "Mix", Order = 1 } }
        };

    [Fact]
    public async Task GetRecipeByIdAsync_WhenNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _recipeRepoMock.Setup(r => r.GetDetailedByIdAsync(1)).ReturnsAsync((Recipe?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetRecipeByIdAsync(1, 1));
    }

    [Fact]
    public async Task GetRecipeByIdAsync_ReturnsDetailResponse()
    {
        // Arrange
        var recipe = new Recipe
        {
            Id = 1, Title = "Pasta",
            User = new User { Id = 1, Username = "chef" },
            Category = new Category { Name = "Italian" },
            Tags = new List<Tag>(),
            Ingredients = new List<Ingredient>(),
            Steps = new List<Step>()
        };
        _recipeRepoMock.Setup(r => r.GetDetailedByIdAsync(1)).ReturnsAsync(recipe);

        // Act
        var result = await _sut.GetRecipeByIdAsync(1, 1);

        // Assert
        Assert.Equal(1, result.Id);
        Assert.Equal("Pasta", result.Title);
        _socialStatsMock.Verify(s => s.ApplyStatsAsync(It.IsAny<RecipeDetailResponse>(), 1), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenCategoryNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _categoryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Category?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.CreateAsync(ValidCreateRequest(), 1));
    }

    [Fact]
    public async Task CreateAsync_WhenCategoryInactive_ThrowsBadRequestException()
    {
        // Arrange
        _categoryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Category { IsActive = false });

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _sut.CreateAsync(ValidCreateRequest(), 1));
    }

    [Fact]
    public async Task CreateAsync_WhenTagsNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _categoryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Category { IsActive = true });
        _tagRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Tag>());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.CreateAsync(ValidCreateRequest(tagIds: new List<int> { 1, 2 }), 1));
    }

    [Fact]
    public async Task CreateAsync_WhenTagInactive_ThrowsBadRequestException()
    {
        // Arrange
        _categoryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Category { IsActive = true });
        _tagRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Tag> { new() { Id = 1, IsActive = false } });

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.CreateAsync(ValidCreateRequest(tagIds: new List<int> { 1 }), 1));
    }

    [Fact]
    public async Task CreateAsync_CreatesRecipeAndSavesChanges()
    {
        // Arrange
        _categoryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Category { IsActive = true });
        _tagRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(new List<Tag>());

        // Act
        await _sut.CreateAsync(ValidCreateRequest(), 5);

        // Assert
        _recipeRepoMock.Verify(r => r.AddAsync(It.Is<Recipe>(rec => rec.UserId == 5 && rec.Title == "Test Recipe")), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenRecipeNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _recipeRepoMock.Setup(r => r.GetDetailedByIdAsync(1)).ReturnsAsync((Recipe?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.UpdateAsync(1, new UpdateRecipeRequest { CategoryId = 1, TagIds = new(), Ingredients = new(), Steps = new() }, 1));
    }

    [Fact]
    public async Task UpdateAsync_WhenNotOwner_ThrowsForbiddenException()
    {
        // Arrange
        _recipeRepoMock.Setup(r => r.GetDetailedByIdAsync(1)).ReturnsAsync(new Recipe { Id = 1, UserId = 2 });

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _sut.UpdateAsync(1, new UpdateRecipeRequest { CategoryId = 1, TagIds = new(), Ingredients = new(), Steps = new() }, 99));
    }

    [Fact]
    public async Task UpdateAsync_WhenCategoryInactive_ThrowsBadRequestException()
    {
        // Arrange
        _recipeRepoMock.Setup(r => r.GetDetailedByIdAsync(1))
            .ReturnsAsync(new Recipe { Id = 1, UserId = 1, Ingredients = new List<Ingredient>(), Steps = new List<Step>(), Tags = new List<Tag>() });
        _categoryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Category { IsActive = false });

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.UpdateAsync(1, new UpdateRecipeRequest { CategoryId = 1, TagIds = new(), Ingredients = new(), Steps = new() }, 1));
    }

    [Fact]
    public async Task DeleteAsync_WhenRecipeNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Recipe?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(1, 1));
    }

    [Fact]
    public async Task DeleteAsync_WhenRecipeIsDeleted_ThrowsNotFoundException()
    {
        // Arrange
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Recipe { IsDeleted = true });

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(1, 1));
    }

    [Fact]
    public async Task DeleteAsync_WhenNotOwner_ThrowsForbiddenException()
    {
        // Arrange
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Recipe { Id = 1, UserId = 2 });

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() => _sut.DeleteAsync(1, 99));
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesRecipeAndSavesChanges()
    {
        // Arrange
        var recipe = new Recipe { Id = 1, UserId = 5 };
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(recipe);

        // Act
        await _sut.DeleteAsync(1, 5);

        // Assert
        Assert.True(recipe.IsDeleted);
        Assert.NotNull(recipe.DeletedAt);
        _recipeRepoMock.Verify(r => r.Update(recipe), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UploadImageAsync_WhenInvalidExtension_ThrowsBadRequestException()
    {
        // Arrange
        using var image = new MemoryStream(new byte[100]);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.UploadImageAsync(1, image, "photo.bmp", 1));
    }

    [Fact]
    public async Task UploadImageAsync_WhenFileTooLarge_ThrowsBadRequestException()
    {
        // Arrange
        using var image = new MemoryStream(new byte[6 * 1024 * 1024]);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.UploadImageAsync(1, image, "photo.jpg", 1));
    }

    [Fact]
    public async Task UploadImageAsync_WhenRecipeNotFound_ThrowsNotFoundException()
    {
        // Arrange
        using var image = new MemoryStream(new byte[100]);
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Recipe?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.UploadImageAsync(1, image, "photo.jpg", 1));
    }

    [Fact]
    public async Task UploadImageAsync_WhenNotOwner_ThrowsForbiddenException()
    {
        // Arrange
        using var image = new MemoryStream(new byte[100]);
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Recipe { Id = 1, UserId = 2 });

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _sut.UploadImageAsync(1, image, "photo.jpg", 99));
    }

    [Fact]
    public async Task DeleteImageAsync_WhenRecipeNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Recipe?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteImageAsync(1, 1));
    }

    [Fact]
    public async Task DeleteImageAsync_WhenNotOwner_ThrowsForbiddenException()
    {
        // Arrange
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Recipe { Id = 1, UserId = 2 });

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() => _sut.DeleteImageAsync(1, 99));
    }

    [Fact]
    public async Task DeleteImageAsync_WhenNoImage_ReturnsEarly()
    {
        // Arrange
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Recipe { Id = 1, UserId = 5, ImagePublicId = null });

        // Act
        await _sut.DeleteImageAsync(1, 5);

        // Assert
        _imageStorageMock.Verify(s => s.DeleteAsync(It.IsAny<string>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
