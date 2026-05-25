using Moq;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Services;

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
}
