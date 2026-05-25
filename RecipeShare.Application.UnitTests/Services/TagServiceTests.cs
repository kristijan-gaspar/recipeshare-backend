using Moq;
using RecipeShare.Application.DTOs.Tags;
using RecipeShare.Application.Exceptions;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Services;
using RecipeShare.Domain.Entities;
using Xunit;

namespace RecipeShare.Application.UnitTests.Services;

public class TagServiceTests
{
    private readonly Mock<ITagRepository> _tagRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly TagService _sut;

    public TagServiceTests()
    {
        _sut = new TagService(
            _tagRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetTagsAsync_ReturnsOnlyActiveTags()
    {
        // Arrange
        var tags = new List<Tag>
        {
            new() { Id = 1, Name = "Active", IsActive = true, Recipes = new List<Recipe>() },
            new() { Id = 2, Name = "Inactive", IsActive = false, Recipes = new List<Recipe>() }
        };
        _tagRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(tags);

        // Act
        var result = await _sut.GetTagsAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Active", result[0].Name);
    }

    [Fact]
    public async Task GetTagByIdAsync_WhenNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _tagRepoMock.Setup(r => r.GetByIdWithRecipesAsync(1)).ReturnsAsync((Tag?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetTagByIdAsync(1));
    }

    [Fact]
    public async Task GetTagByIdAsync_ReturnsCorrectResponse()
    {
        // Arrange
        var tag = new Tag { Id = 1, Name = "Vegan", Recipes = new List<Recipe> { new(), new(), new() } };
        _tagRepoMock.Setup(r => r.GetByIdWithRecipesAsync(1)).ReturnsAsync(tag);

        // Act
        var result = await _sut.GetTagByIdAsync(1);

        // Assert
        Assert.Equal(1, result.Id);
        Assert.Equal("Vegan", result.Name);
        Assert.Equal(3, result.RecipeCount);
    }

    [Fact]
    public async Task GetAdminTagsAsync_ReturnsAllTags()
    {
        // Arrange
        var tags = new List<Tag>
        {
            new() { Id = 1, Name = "A", IsActive = true, Recipes = new List<Recipe>() },
            new() { Id = 2, Name = "B", IsActive = false, Recipes = new List<Recipe>() }
        };
        _tagRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(tags);

        // Act
        var result = await _sut.GetAdminTagsAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAdminTagByIdAsync_WhenNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _tagRepoMock.Setup(r => r.GetByIdWithRecipesAsync(99)).ReturnsAsync((Tag?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetAdminTagByIdAsync(99));
    }

    [Fact]
    public async Task GetAdminTagByIdAsync_ReturnsCorrectResponse()
    {
        // Arrange
        var tag = new Tag { Id = 1, Name = "Gluten-Free", IsActive = true, Recipes = new List<Recipe>() };
        _tagRepoMock.Setup(r => r.GetByIdWithRecipesAsync(1)).ReturnsAsync(tag);

        // Act
        var result = await _sut.GetAdminTagByIdAsync(1);

        // Assert
        Assert.Equal(1, result.Id);
        Assert.Equal("Gluten-Free", result.Name);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task CreateTagAsync_WhenNameExists_ThrowsBadRequestException()
    {
        // Arrange
        _tagRepoMock.Setup(r => r.NameExistsAsync("Vegan")).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.CreateTagAsync(new CreateTagRequest { Name = "Vegan" }));
    }

    [Fact]
    public async Task CreateTagAsync_CreatesTagAndSavesChanges()
    {
        // Arrange
        _tagRepoMock.Setup(r => r.NameExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

        // Act
        await _sut.CreateTagAsync(new CreateTagRequest { Name = "  Keto  " });

        // Assert
        _tagRepoMock.Verify(r => r.AddAsync(It.Is<Tag>(t => t.Name == "Keto")), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTagAsync_WhenNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _tagRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Tag?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.UpdateTagAsync(1, new CreateTagRequest { Name = "New" }));
    }

    [Fact]
    public async Task UpdateTagAsync_WhenNameExistsForDifferentTag_ThrowsBadRequestException()
    {
        // Arrange
        var tag = new Tag { Id = 1, Name = "OldName" };
        _tagRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tag);
        _tagRepoMock.Setup(r => r.NameExistsAsync("NewName")).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.UpdateTagAsync(1, new CreateTagRequest { Name = "NewName" }));
    }

    [Fact]
    public async Task UpdateTagAsync_UpdatesTagAndSavesChanges()
    {
        // Arrange
        var tag = new Tag { Id = 1, Name = "Old" };
        _tagRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tag);
        _tagRepoMock.Setup(r => r.NameExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

        // Act
        await _sut.UpdateTagAsync(1, new CreateTagRequest { Name = "  New  " });

        // Assert
        _tagRepoMock.Verify(r => r.Update(It.Is<Tag>(t => t.Name == "New")), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteTagAsync_WhenNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _tagRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Tag?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteTagAsync(1));
    }

    [Fact]
    public async Task DeleteTagAsync_DeletesTagAndSavesChanges()
    {
        // Arrange
        var tag = new Tag { Id = 1, Name = "Spicy" };
        _tagRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tag);

        // Act
        await _sut.DeleteTagAsync(1);

        // Assert
        _tagRepoMock.Verify(r => r.Delete(tag), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ToggleActiveAsync_WhenNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _tagRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Tag?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.ToggleActiveAsync(1));
    }

    [Fact]
    public async Task ToggleActiveAsync_TogglesIsActiveAndSavesChanges()
    {
        // Arrange
        var tag = new Tag { Id = 1, IsActive = true };
        _tagRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tag);

        // Act
        await _sut.ToggleActiveAsync(1);

        // Assert
        Assert.False(tag.IsActive);
        _tagRepoMock.Verify(r => r.Update(tag), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
