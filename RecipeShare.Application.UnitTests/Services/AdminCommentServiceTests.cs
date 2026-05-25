using Moq;
using RecipeShare.Application.Exceptions;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Services;
using RecipeShare.Domain.Entities;
using Xunit;

namespace RecipeShare.Application.UnitTests.Services;

public class AdminCommentServiceTests
{
    private readonly Mock<ICommentRepository> _commentRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly AdminCommentService _sut;

    public AdminCommentServiceTests()
    {
        _sut = new AdminCommentService(
            _commentRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task SoftDeleteAsync_WhenCommentNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _commentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Comment?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.SoftDeleteAsync(1));
    }

    [Fact]
    public async Task SoftDeleteAsync_WhenCommentAlreadyDeleted_ThrowsNotFoundException()
    {
        // Arrange
        _commentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Comment { IsDeleted = true });

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.SoftDeleteAsync(1));
    }

    [Fact]
    public async Task SoftDeleteAsync_SetsIsDeletedAndSavesChanges()
    {
        // Arrange
        var comment = new Comment { Id = 1 };
        _commentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);

        // Act
        await _sut.SoftDeleteAsync(1);

        // Assert
        Assert.True(comment.IsDeleted);
        Assert.NotNull(comment.DeletedAt);
        _commentRepoMock.Verify(r => r.Update(comment), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RestoreAsync_WhenCommentNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _commentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Comment?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.RestoreAsync(1));
    }

    [Fact]
    public async Task RestoreAsync_WhenCommentNotDeleted_ThrowsBadRequestException()
    {
        // Arrange
        _commentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Comment { IsDeleted = false });

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _sut.RestoreAsync(1));
    }

    [Fact]
    public async Task RestoreAsync_ClearsIsDeletedAndSavesChanges()
    {
        // Arrange
        var comment = new Comment { Id = 1, IsDeleted = true, DeletedAt = DateTime.UtcNow.AddDays(-1) };
        _commentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);

        // Act
        await _sut.RestoreAsync(1);

        // Assert
        Assert.False(comment.IsDeleted);
        Assert.Null(comment.DeletedAt);
        _commentRepoMock.Verify(r => r.Update(comment), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
