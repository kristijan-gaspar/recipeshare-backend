using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RecipeShare.Application.DTOs.Comments;
using RecipeShare.Application.Exceptions;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Application.Mappings;
using RecipeShare.Application.Services;
using RecipeShare.Domain.Entities;
using Xunit;

namespace RecipeShare.Application.UnitTests.Services;

public class CommentServiceTests
{
    private readonly Mock<ICommentRepository> _commentRepoMock = new();
    private readonly Mock<IRecipeRepository> _recipeRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<INotificationService> _notificationServiceMock = new();

    private readonly CommentService _sut;

    static CommentServiceTests() => MappingConfig.Configure();

    public CommentServiceTests()
    {
        _sut = new CommentService(
            _commentRepoMock.Object,
            _recipeRepoMock.Object,
            _unitOfWorkMock.Object,
            _notificationServiceMock.Object,
            new NullLogger<CommentService>());
    }

    [Fact]
    public async Task GetPagedAsync_WhenNoComments_ReturnsEmptyResponse()
    {
        // Arrange
        _commentRepoMock
            .Setup(r => r.GetCursorPagedByRecipeAsync(1, It.IsAny<CommentQueryParameters>()))
            .ReturnsAsync((new List<Comment>(), false));

        // Act
        var result = await _sut.GetPagedAsync(1, new CommentQueryParameters());

        // Assert
        Assert.Empty(result.Items);
        Assert.False(result.HasMore);
        Assert.Null(result.NextCursor);
    }

    [Fact]
    public async Task GetPagedAsync_WhenHasMore_SetsNextCursorToLastItemId()
    {
        // Arrange
        var comments = new List<Comment>
        {
            new() { Id = 1, Content = "First", User = new User { Id = 1, Username = "u1" } },
            new() { Id = 2, Content = "Second", User = new User { Id = 2, Username = "u2" } }
        };
        _commentRepoMock
            .Setup(r => r.GetCursorPagedByRecipeAsync(1, It.IsAny<CommentQueryParameters>()))
            .ReturnsAsync((comments, true));

        // Act
        var result = await _sut.GetPagedAsync(1, new CommentQueryParameters());

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.True(result.HasMore);
        Assert.Equal(2, result.NextCursor);
    }

    [Fact]
    public async Task CreateAsync_WhenRecipeNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Recipe?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.CreateAsync(1, 1, new CommentRequest { Content = "test" }, "actor"));
    }

    [Fact]
    public async Task CreateAsync_WhenRecipeIsDeleted_ThrowsNotFoundException()
    {
        // Arrange
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Recipe { IsDeleted = true });

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.CreateAsync(1, 1, new CommentRequest { Content = "test" }, "actor"));
    }

    [Fact]
    public async Task CreateAsync_AddsCommentAndSavesChanges()
    {
        // Arrange
        var recipe = new Recipe { Id = 1, UserId = 2, Title = "Pizza" };
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(recipe);
        _commentRepoMock
            .Setup(r => r.GetByIdWithUserAsync(It.IsAny<int>()))
            .ReturnsAsync(new Comment { Id = 1, Content = "Great!", UserId = 1, User = new User { Id = 1, Username = "actor" } });

        // Act
        await _sut.CreateAsync(1, 1, new CommentRequest { Content = "Great!" }, "actor");

        // Assert
        _commentRepoMock.Verify(r => r.AddAsync(It.Is<Comment>(c => c.Content == "Great!" && c.RecipeId == 1 && c.UserId == 1)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenDifferentUser_SendsCommentNotification()
    {
        // Arrange
        var recipe = new Recipe { Id = 1, UserId = 2, Title = "Pizza" };
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(recipe);
        _commentRepoMock
            .Setup(r => r.GetByIdWithUserAsync(It.IsAny<int>()))
            .ReturnsAsync(new Comment { Id = 1, Content = "Nice", UserId = 1, User = new User { Id = 1, Username = "actor" } });

        // Act
        await _sut.CreateAsync(1, 1, new CommentRequest { Content = "Nice" }, "actor");

        // Assert
        _notificationServiceMock.Verify(n => n.SendCommentNotificationAsync(2, "actor", "Pizza", 1), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenSameUser_DoesNotSendNotification()
    {
        // Arrange
        var recipe = new Recipe { Id = 1, UserId = 1, Title = "Pizza" };
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(recipe);
        _commentRepoMock
            .Setup(r => r.GetByIdWithUserAsync(It.IsAny<int>()))
            .ReturnsAsync(new Comment { Id = 1, Content = "Nice", UserId = 1, User = new User { Id = 1, Username = "actor" } });

        // Act
        await _sut.CreateAsync(1, 1, new CommentRequest { Content = "Nice" }, "actor");

        // Assert
        _notificationServiceMock.Verify(
            n => n.SendCommentNotificationAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenNotificationFails_DoesNotPropagate()
    {
        // Arrange
        var recipe = new Recipe { Id = 1, UserId = 2, Title = "Pizza" };
        _recipeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(recipe);
        _commentRepoMock
            .Setup(r => r.GetByIdWithUserAsync(It.IsAny<int>()))
            .ReturnsAsync(new Comment { Id = 1, Content = "test", UserId = 1, User = new User { Id = 1, Username = "actor" } });
        _notificationServiceMock
            .Setup(n => n.SendCommentNotificationAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception("push failed"));

        // Act
        var ex = await Record.ExceptionAsync(() =>
            _sut.CreateAsync(1, 1, new CommentRequest { Content = "test" }, "actor"));

        // Assert
        Assert.Null(ex);
    }

    [Fact]
    public async Task UpdateAsync_WhenCommentNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _commentRepoMock.Setup(r => r.GetByIdWithUserAsync(1)).ReturnsAsync((Comment?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.UpdateAsync(1, 1, new CommentRequest { Content = "edited" }));
    }

    [Fact]
    public async Task UpdateAsync_WhenNotOwner_ThrowsForbiddenException()
    {
        // Arrange
        var comment = new Comment { Id = 1, UserId = 2, Content = "original", User = new User { Id = 2, Username = "owner" } };
        _commentRepoMock.Setup(r => r.GetByIdWithUserAsync(1)).ReturnsAsync(comment);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _sut.UpdateAsync(1, 99, new CommentRequest { Content = "edited" }));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesContentAndSavesChanges()
    {
        // Arrange
        var comment = new Comment { Id = 1, UserId = 5, Content = "old content", User = new User { Id = 5, Username = "user5" } };
        _commentRepoMock.Setup(r => r.GetByIdWithUserAsync(1)).ReturnsAsync(comment);

        // Act
        var result = await _sut.UpdateAsync(1, 5, new CommentRequest { Content = "new content" });

        // Assert
        Assert.Equal("new content", result.Content);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenCommentNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _commentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Comment?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(1, 1));
    }

    [Fact]
    public async Task DeleteAsync_WhenCommentAlreadyDeleted_ThrowsNotFoundException()
    {
        // Arrange
        _commentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Comment { Id = 1, IsDeleted = true });

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(1, 1));
    }

    [Fact]
    public async Task DeleteAsync_WhenNotOwner_ThrowsForbiddenException()
    {
        // Arrange
        var comment = new Comment { Id = 1, UserId = 2 };
        _commentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() => _sut.DeleteAsync(1, 99));
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesComment_AndSavesChanges()
    {
        // Arrange
        var comment = new Comment { Id = 1, UserId = 5 };
        _commentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);

        // Act
        await _sut.DeleteAsync(1, 5);

        // Assert
        Assert.True(comment.IsDeleted);
        Assert.NotNull(comment.DeletedAt);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
