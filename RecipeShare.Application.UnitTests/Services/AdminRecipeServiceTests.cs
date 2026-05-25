using Moq;
using RecipeShare.Application.Exceptions;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Mappings;
using RecipeShare.Application.Services;
using RecipeShare.Domain.Entities;
using Xunit;

namespace RecipeShare.Application.UnitTests.Services;

public class AdminRecipeServiceTests
{
    private readonly Mock<IRecipeRepository> _recipeRepoMock = new();
    private readonly Mock<ICommentRepository> _commentRepoMock = new();
    private readonly Mock<ILikeRepository> _likeRepoMock = new();
    private readonly Mock<IRatingRepository> _ratingRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly AdminRecipeService _sut;

    static AdminRecipeServiceTests() => MappingConfig.Configure();

    public AdminRecipeServiceTests()
    {
        _sut = new AdminRecipeService(
            _recipeRepoMock.Object,
            _commentRepoMock.Object,
            _likeRepoMock.Object,
            _ratingRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetRecipeAsync_WhenNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _recipeRepoMock.Setup(r => r.GetDetailedByIdForAdminAsync(1)).ReturnsAsync((Recipe?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetRecipeAsync(1));
    }

    [Fact]
    public async Task GetRecipeAsync_ReturnsRecipeWithStats()
    {
        // Arrange
        var recipe = new Recipe
        {
            Id = 1, Title = "Tacos",
            User = new User { Id = 1, Username = "chef" },
            Category = new Category { Name = "Mexican" },
            Tags = new List<Tag>(),
            Ingredients = new List<Ingredient>(),
            Steps = new List<Step>()
        };
        _recipeRepoMock.Setup(r => r.GetDetailedByIdForAdminAsync(1)).ReturnsAsync(recipe);
        _likeRepoMock.Setup(r => r.GetCountByRecipeAsync(1)).ReturnsAsync(42);
        _ratingRepoMock.Setup(r => r.GetStatsByRecipeAsync(1)).ReturnsAsync((4.5, 20));
        _commentRepoMock.Setup(r => r.GetCountByRecipeAsync(1)).ReturnsAsync(7);
        _commentRepoMock.Setup(r => r.GetAllByRecipeForAdminAsync(1)).ReturnsAsync(new List<Comment>());

        // Act
        var result = await _sut.GetRecipeAsync(1);

        // Assert
        Assert.Equal(1, result.Id);
        Assert.Equal("Tacos", result.Title);
        Assert.Equal(42, result.LikeCount);
        Assert.Equal(4.5, result.AverageRating);
        Assert.Equal(20, result.RatingCount);
        Assert.Equal(7, result.CommentCount);
    }

    [Fact]
    public async Task SoftDeleteAsync_WhenRecipeNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Recipe?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.SoftDeleteAsync(1));
    }

    [Fact]
    public async Task SoftDeleteAsync_WhenRecipeAlreadyDeleted_ThrowsNotFoundException()
    {
        // Arrange
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Recipe { IsDeleted = true });

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.SoftDeleteAsync(1));
    }

    [Fact]
    public async Task SoftDeleteAsync_SetsIsDeletedAndSavesChanges()
    {
        // Arrange
        var recipe = new Recipe { Id = 1 };
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(recipe);

        // Act
        await _sut.SoftDeleteAsync(1);

        // Assert
        Assert.True(recipe.IsDeleted);
        Assert.NotNull(recipe.DeletedAt);
        _recipeRepoMock.Verify(r => r.Update(recipe), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RestoreAsync_WhenRecipeNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Recipe?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.RestoreAsync(1));
    }

    [Fact]
    public async Task RestoreAsync_WhenRecipeNotDeleted_ThrowsBadRequestException()
    {
        // Arrange
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Recipe { IsDeleted = false });

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _sut.RestoreAsync(1));
    }

    [Fact]
    public async Task RestoreAsync_ClearsIsDeletedAndSavesChanges()
    {
        // Arrange
        var recipe = new Recipe { Id = 1, IsDeleted = true, DeletedAt = DateTime.UtcNow.AddDays(-1) };
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(recipe);

        // Act
        await _sut.RestoreAsync(1);

        // Assert
        Assert.False(recipe.IsDeleted);
        Assert.Null(recipe.DeletedAt);
        _recipeRepoMock.Verify(r => r.Update(recipe), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ToggleFeaturedAsync_WhenRecipeNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Recipe?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.ToggleFeaturedAsync(1));
    }

    [Fact]
    public async Task ToggleFeaturedAsync_WhenRecipeIsDeleted_ThrowsNotFoundException()
    {
        // Arrange
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Recipe { IsDeleted = true });

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.ToggleFeaturedAsync(1));
    }

    [Fact]
    public async Task ToggleFeaturedAsync_TogglesIsFeaturedAndSavesChanges()
    {
        // Arrange
        var recipe = new Recipe { Id = 1, IsFeatured = false };
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(recipe);

        // Act
        await _sut.ToggleFeaturedAsync(1);

        // Assert
        Assert.True(recipe.IsFeatured);
        _recipeRepoMock.Verify(r => r.Update(recipe), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
