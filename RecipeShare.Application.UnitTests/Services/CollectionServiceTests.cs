using Moq;
using RecipeShare.Application.DTOs.Collections;
using RecipeShare.Application.Exceptions;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Services;
using RecipeShare.Domain.Entities;
using Xunit;

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

    [Fact]
    public async Task CreateAsync_WhenNameExists_ThrowsBadRequestException()
    {
        // Arrange
        _collectionRepoMock.Setup(r => r.NameExistsForUserAsync(1, "Favourites")).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.CreateAsync(new CreateCollectionRequest { Name = "Favourites" }, 1));
    }

    [Fact]
    public async Task CreateAsync_CreatesCollectionAndSavesChanges()
    {
        // Arrange
        _collectionRepoMock.Setup(r => r.NameExistsForUserAsync(1, It.IsAny<string>())).ReturnsAsync(false);

        // Act
        await _sut.CreateAsync(new CreateCollectionRequest { Name = "  Weeknight Meals  " }, 1);

        // Assert
        _collectionRepoMock.Verify(r => r.AddAsync(It.Is<Collection>(c => c.Name == "Weeknight Meals" && c.UserId == 1)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenCollectionNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _collectionRepoMock.Setup(r => r.GetByIdAndUserIdAsync(1, 1)).ReturnsAsync((Collection?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(1, 1));
    }

    [Fact]
    public async Task DeleteAsync_DeletesCollectionAndSavesChanges()
    {
        // Arrange
        var collection = new Collection { Id = 1, UserId = 1 };
        _collectionRepoMock.Setup(r => r.GetByIdAndUserIdAsync(1, 1)).ReturnsAsync(collection);

        // Act
        await _sut.DeleteAsync(1, 1);

        // Assert
        _collectionRepoMock.Verify(r => r.Delete(collection), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddRecipeAsync_WhenCollectionNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _collectionRepoMock.Setup(r => r.GetByIdAndUserIdAsync(1, 1)).ReturnsAsync((Collection?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.AddRecipeAsync(1, 1, 1));
    }

    [Fact]
    public async Task AddRecipeAsync_WhenRecipeNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _collectionRepoMock.Setup(r => r.GetByIdAndUserIdAsync(1, 1))
            .ReturnsAsync(new Collection { Id = 1, CollectionRecipes = new List<CollectionRecipe>() });
        _recipeRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((Recipe?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.AddRecipeAsync(1, 5, 1));
    }

    [Fact]
    public async Task AddRecipeAsync_WhenRecipeIsDeleted_ThrowsNotFoundException()
    {
        // Arrange
        _collectionRepoMock.Setup(r => r.GetByIdAndUserIdAsync(1, 1))
            .ReturnsAsync(new Collection { Id = 1, CollectionRecipes = new List<CollectionRecipe>() });
        _recipeRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new Recipe { IsDeleted = true });

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.AddRecipeAsync(1, 5, 1));
    }

    [Fact]
    public async Task AddRecipeAsync_WhenAlreadyAdded_ThrowsBadRequestException()
    {
        // Arrange
        var collection = new Collection
        {
            Id = 1,
            CollectionRecipes = new List<CollectionRecipe> { new() { RecipeId = 5 } }
        };
        _collectionRepoMock.Setup(r => r.GetByIdAndUserIdAsync(1, 1)).ReturnsAsync(collection);
        _recipeRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new Recipe { Id = 5 });

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _sut.AddRecipeAsync(1, 5, 1));
    }

    [Fact]
    public async Task AddRecipeAsync_AddsRecipeAndSavesChanges()
    {
        // Arrange
        var collection = new Collection { Id = 1, CollectionRecipes = new List<CollectionRecipe>() };
        _collectionRepoMock.Setup(r => r.GetByIdAndUserIdAsync(1, 1)).ReturnsAsync(collection);
        _recipeRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new Recipe { Id = 5 });

        // Act
        await _sut.AddRecipeAsync(1, 5, 1);

        // Assert
        Assert.Single(collection.CollectionRecipes);
        Assert.Equal(5, collection.CollectionRecipes.First().RecipeId);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveRecipeAsync_WhenCollectionNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _collectionRepoMock.Setup(r => r.GetByIdAndUserIdAsync(1, 1)).ReturnsAsync((Collection?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.RemoveRecipeAsync(1, 5, 1));
    }

    [Fact]
    public async Task RemoveRecipeAsync_WhenRecipeNotInCollection_ThrowsNotFoundException()
    {
        // Arrange
        var collection = new Collection { Id = 1, CollectionRecipes = new List<CollectionRecipe>() };
        _collectionRepoMock.Setup(r => r.GetByIdAndUserIdAsync(1, 1)).ReturnsAsync(collection);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.RemoveRecipeAsync(1, 5, 1));
    }

    [Fact]
    public async Task RemoveRecipeAsync_RemovesRecipeAndSavesChanges()
    {
        // Arrange
        var cr = new CollectionRecipe { RecipeId = 5 };
        var collection = new Collection { Id = 1, CollectionRecipes = new List<CollectionRecipe> { cr } };
        _collectionRepoMock.Setup(r => r.GetByIdAndUserIdAsync(1, 1)).ReturnsAsync(collection);

        // Act
        await _sut.RemoveRecipeAsync(1, 5, 1);

        // Assert
        Assert.Empty(collection.CollectionRecipes);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByUserAsync_ReturnsMappedCollections()
    {
        // Arrange
        var collections = new List<Collection>
        {
            new() { Id = 1, Name = "Faves", CollectionRecipes = new List<CollectionRecipe> { new(), new() } },
            new() { Id = 2, Name = "Quick Meals", CollectionRecipes = new List<CollectionRecipe>() }
        };
        _collectionRepoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(collections);

        // Act
        var result = await _sut.GetByUserAsync(1);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Faves", result[0].Name);
        Assert.Equal(2, result[0].RecipeCount);
        Assert.Equal(0, result[1].RecipeCount);
    }

    [Fact]
    public async Task GetRecipesAsync_WhenCollectionNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _collectionRepoMock.Setup(r => r.GetWithRecipesAsync(1)).ReturnsAsync((Collection?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetRecipesAsync(1, 1));
    }

    [Fact]
    public async Task GetRecipesAsync_WhenNotOwner_ThrowsForbiddenException()
    {
        // Arrange
        var collection = new Collection { Id = 1, UserId = 2, CollectionRecipes = new List<CollectionRecipe>() };
        _collectionRepoMock.Setup(r => r.GetWithRecipesAsync(1)).ReturnsAsync(collection);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() => _sut.GetRecipesAsync(1, 99));
    }

    [Fact]
    public async Task GetRecipesAsync_ReturnsMappedRecipes()
    {
        // Arrange
        var recipe = new Recipe
        {
            Id = 10,
            Title = "Pasta",
            User = new User { Id = 1, Username = "chef" },
            Category = new Category { Name = "Italian" },
            Tags = new List<Tag>()
        };
        var collection = new Collection
        {
            Id = 1, UserId = 5,
            CollectionRecipes = new List<CollectionRecipe> { new() { Recipe = recipe } }
        };
        _collectionRepoMock.Setup(r => r.GetWithRecipesAsync(1)).ReturnsAsync(collection);

        // Act
        var result = await _sut.GetRecipesAsync(1, 5);

        // Assert
        Assert.Single(result);
        Assert.Equal("Pasta", result[0].Title);
    }
}
