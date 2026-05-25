using Moq;
using RecipeShare.Application.DTOs.Reports;
using RecipeShare.Application.Exceptions;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Application.Mappings;
using RecipeShare.Application.Services;
using RecipeShare.Domain.Entities;
using RecipeShare.Domain.Enums;
using Xunit;

namespace RecipeShare.Application.UnitTests.Services;

public class ReportServiceTests
{
    private readonly Mock<IReportRepository> _reportRepoMock = new();
    private readonly Mock<IRecipeRepository> _recipeRepoMock = new();
    private readonly Mock<ICommentRepository> _commentRepoMock = new();
    private readonly Mock<IAdminRecipeService> _adminRecipeServiceMock = new();
    private readonly Mock<IAdminCommentService> _adminCommentServiceMock = new();
    private readonly Mock<IAdminUserService> _adminUserServiceMock = new();
    private readonly Mock<INotificationService> _notificationServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly ReportService _sut;

    static ReportServiceTests() => MappingConfig.Configure();

    public ReportServiceTests()
    {
        _sut = new ReportService(
            _reportRepoMock.Object,
            _recipeRepoMock.Object,
            _commentRepoMock.Object,
            _adminRecipeServiceMock.Object,
            _adminCommentServiceMock.Object,
            _adminUserServiceMock.Object,
            _notificationServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WhenRecipeTargetNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _recipeRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync((Recipe?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.CreateAsync(1, new CreateReportRequest { TargetType = ReportTargetType.Recipe, TargetId = 10, Reason = ReportReason.Spam }));
    }

    [Fact]
    public async Task CreateAsync_WhenRecipeTargetIsDeleted_ThrowsNotFoundException()
    {
        // Arrange
        _recipeRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new Recipe { IsDeleted = true });

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.CreateAsync(1, new CreateReportRequest { TargetType = ReportTargetType.Recipe, TargetId = 10, Reason = ReportReason.Spam }));
    }

    [Fact]
    public async Task CreateAsync_WhenCommentTargetNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _commentRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((Comment?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.CreateAsync(1, new CreateReportRequest { TargetType = ReportTargetType.Comment, TargetId = 5, Reason = ReportReason.Spam }));
    }

    [Fact]
    public async Task CreateAsync_WhenCommentTargetIsDeleted_ThrowsNotFoundException()
    {
        // Arrange
        _commentRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new Comment { IsDeleted = true });

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.CreateAsync(1, new CreateReportRequest { TargetType = ReportTargetType.Comment, TargetId = 5, Reason = ReportReason.Spam }));
    }

    [Fact]
    public async Task CreateAsync_WhenReportingOwnContent_ThrowsBadRequestException()
    {
        // Arrange
        _recipeRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new Recipe { Id = 10, UserId = 1 });

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.CreateAsync(1, new CreateReportRequest { TargetType = ReportTargetType.Recipe, TargetId = 10, Reason = ReportReason.Spam }));
    }

    [Fact]
    public async Task CreateAsync_WhenAlreadyReported_ThrowsBadRequestException()
    {
        // Arrange
        _recipeRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new Recipe { Id = 10, UserId = 2 });
        _reportRepoMock.Setup(r => r.ExistsAsync(1, ReportTargetType.Recipe, 10)).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.CreateAsync(1, new CreateReportRequest { TargetType = ReportTargetType.Recipe, TargetId = 10, Reason = ReportReason.Spam }));
    }

    [Fact]
    public async Task CreateAsync_CreatesReportAndSavesChanges()
    {
        // Arrange
        _recipeRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new Recipe { Id = 10, UserId = 2 });
        _reportRepoMock.Setup(r => r.ExistsAsync(1, ReportTargetType.Recipe, 10)).ReturnsAsync(false);

        // Act
        await _sut.CreateAsync(1, new CreateReportRequest { TargetType = ReportTargetType.Recipe, TargetId = 10, Reason = ReportReason.Spam });

        // Assert
        _reportRepoMock.Verify(r => r.AddAsync(It.Is<Report>(rep => rep.ReporterId == 1 && rep.TargetId == 10)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _reportRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync((Report?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(1));
    }

    [Fact]
    public async Task ResolveAsync_WhenReportNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _reportRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync((Report?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.ResolveAsync(1, 99, new ResolveReportRequest()));
    }

    [Fact]
    public async Task ResolveAsync_WhenAlreadyProcessed_ThrowsBadRequestException()
    {
        // Arrange
        _reportRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(new Report { Status = ReportStatus.Resolved });

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.ResolveAsync(1, 99, new ResolveReportRequest()));
    }

    [Fact]
    public async Task ResolveAsync_WithSoftDeleteContentAction_CallsAdminRecipeService()
    {
        // Arrange
        var report = new Report { Id = 1, Status = ReportStatus.Pending, TargetType = ReportTargetType.Recipe, TargetId = 10, ReportedUserId = 2 };
        _reportRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(report);

        // Act
        await _sut.ResolveAsync(1, 99, new ResolveReportRequest { ContentAction = AdminAction.SoftDelete, UserAction = AdminAction.None });

        // Assert
        _adminRecipeServiceMock.Verify(s => s.SoftDeleteAsync(10), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResolveAsync_WithWarningUserAction_SendsWarningNotification()
    {
        // Arrange
        var report = new Report { Id = 1, Status = ReportStatus.Pending, TargetType = ReportTargetType.Recipe, TargetId = 10, ReportedUserId = 2, Reason = ReportReason.Spam };
        _reportRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(report);

        // Act
        await _sut.ResolveAsync(1, 99, new ResolveReportRequest { ContentAction = AdminAction.None, UserAction = AdminAction.Warning });

        // Assert
        _notificationServiceMock.Verify(n => n.SendWarningNotificationAsync(2, ReportReason.Spam), Times.Once);
    }

    [Fact]
    public async Task DismissAsync_WhenReportNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _reportRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Report?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DismissAsync(1, 99));
    }

    [Fact]
    public async Task DismissAsync_WhenAlreadyProcessed_ThrowsBadRequestException()
    {
        // Arrange
        _reportRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Report { Status = ReportStatus.Dismissed });

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _sut.DismissAsync(1, 99));
    }

    [Fact]
    public async Task DismissAsync_SetsStatusToDismissedAndSavesChanges()
    {
        // Arrange
        var report = new Report { Id = 1, Status = ReportStatus.Pending };
        _reportRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(report);

        // Act
        await _sut.DismissAsync(1, 99);

        // Assert
        Assert.Equal(ReportStatus.Dismissed, report.Status);
        Assert.Equal(99, report.ResolvedByAdminId);
        _reportRepoMock.Verify(r => r.Update(report), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
