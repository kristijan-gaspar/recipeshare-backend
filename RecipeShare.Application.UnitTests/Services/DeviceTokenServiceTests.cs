using Moq;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Services;

namespace RecipeShare.Application.UnitTests.Services;

public class DeviceTokenServiceTests
{
    private readonly Mock<IDeviceTokenRepository> _deviceTokenRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly DeviceTokenService _sut;

    public DeviceTokenServiceTests()
    {
        _sut = new DeviceTokenService(
            _deviceTokenRepoMock.Object,
            _unitOfWorkMock.Object);
    }
}
