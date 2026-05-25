using Moq;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Services;

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
}
