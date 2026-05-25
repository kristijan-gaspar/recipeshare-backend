using Moq;
using RecipeShare.Application.DTOs.Ratings;
using RecipeShare.Application.Exceptions;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Services;
using RecipeShare.Domain.Entities;
using Xunit;

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

    [Fact]
    public async Task RateAsync_WhenRecipeNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Recipe?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.RateAsync(1, 1, new RateRecipeRequest { Value = 4 }));
    }

    [Fact]
    public async Task RateAsync_WhenRecipeIsDeleted_ThrowsNotFoundException()
    {
        // Arrange
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Recipe { IsDeleted = true });

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.RateAsync(1, 1, new RateRecipeRequest { Value = 4 }));
    }

    [Fact]
    public async Task RateAsync_WhenOwnRecipe_ThrowsBadRequestException()
    {
        // Arrange
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Recipe { Id = 1, UserId = 5 });

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.RateAsync(1, 5, new RateRecipeRequest { Value = 4 }));
    }

    [Fact]
    public async Task RateAsync_WhenExistingRating_UpdatesValueAndSavesChanges()
    {
        // Arrange
        var recipe = new Recipe { Id = 1, UserId = 2 };
        var existing = new Rating { UserId = 1, RecipeId = 1, Value = 3 };
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(recipe);
        _ratingRepoMock.Setup(r => r.GetByUserAndRecipeAsync(1, 1)).ReturnsAsync(existing);
        _ratingRepoMock.Setup(r => r.GetStatsByRecipeAsync(1)).ReturnsAsync((4.5, 10));

        // Act
        var result = await _sut.RateAsync(1, 1, new RateRecipeRequest { Value = 5 });

        // Assert
        Assert.Equal(5, existing.Value);
        Assert.Equal(5, result.MyRating);
        Assert.Equal(4.5, result.AverageRating);
        Assert.Equal(10, result.RatingCount);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RateAsync_WhenNoExistingRating_AddsNewRatingAndSavesChanges()
    {
        // Arrange
        var recipe = new Recipe { Id = 1, UserId = 2 };
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(recipe);
        _ratingRepoMock.Setup(r => r.GetByUserAndRecipeAsync(1, 1)).ReturnsAsync((Rating?)null);
        _ratingRepoMock.Setup(r => r.GetStatsByRecipeAsync(1)).ReturnsAsync((3.0, 5));

        // Act
        var result = await _sut.RateAsync(1, 1, new RateRecipeRequest { Value = 4 });

        // Assert
        _ratingRepoMock.Verify(r => r.AddAsync(It.Is<Rating>(ra => ra.UserId == 1 && ra.RecipeId == 1 && ra.Value == 4)), Times.Once);
        Assert.Equal(4, result.MyRating);
        Assert.Equal(3.0, result.AverageRating);
        Assert.Equal(5, result.RatingCount);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
