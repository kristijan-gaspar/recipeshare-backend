using Moq;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Application.Services;

namespace RecipeShare.Application.UnitTests.Services;

public class RecipeResponseMapperTests
{
    private readonly Mock<IRecipeSocialStatsService> _socialStatsMock = new();

    private readonly RecipeResponseMapper _sut;

    public RecipeResponseMapperTests()
    {
        _sut = new RecipeResponseMapper(
            _socialStatsMock.Object);
    }
}
