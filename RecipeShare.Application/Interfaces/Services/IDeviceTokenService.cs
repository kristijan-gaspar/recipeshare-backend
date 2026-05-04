using RecipeShare.Application.DTOs.Notifications;

namespace RecipeShare.Application.Interfaces.Services;

public interface IDeviceTokenService
{
    Task RegisterAsync(int userId, RegisterDeviceTokenRequest request);
    Task UnregisterAsync(string token);
}
