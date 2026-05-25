using Moq;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Application.Services;

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
}
