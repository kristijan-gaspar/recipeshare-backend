using Moq;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Application.Services;

namespace RecipeShare.Application.UnitTests.Services;

public class NotificationServiceTests
{
    private readonly Mock<IDeviceTokenRepository> _deviceTokenRepoMock = new();
    private readonly Mock<IPushNotificationSender> _senderMock = new();

    private readonly NotificationService _sut;

    public NotificationServiceTests()
    {
        _sut = new NotificationService(
            _deviceTokenRepoMock.Object,
            _senderMock.Object);
    }
}
