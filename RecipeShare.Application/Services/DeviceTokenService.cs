using RecipeShare.Application.DTOs.Notifications;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Domain.Entities;

namespace RecipeShare.Application.Services;

public class DeviceTokenService : IDeviceTokenService
{
    private readonly IDeviceTokenRepository _deviceTokenRepo;
    private readonly IUnitOfWork _unitOfWork;

    public DeviceTokenService(IDeviceTokenRepository deviceTokenRepo, IUnitOfWork unitOfWork)
    {
        _deviceTokenRepo = deviceTokenRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task RegisterAsync(int userId, RegisterDeviceTokenRequest request)
    {
        var existing = await _deviceTokenRepo.GetByTokenAsync(request.Token);

        if (existing != null)
        {
            if (existing.UserId == userId)
                return;

            existing.UserId = userId;
            existing.CreatedAt = DateTime.UtcNow;
        }
        else
        {
            await _deviceTokenRepo.AddAsync(new DeviceToken
            {
                Token = request.Token,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UnregisterAsync(string token)
    {
        await _deviceTokenRepo.DeleteByTokenAsync(token);
        await _unitOfWork.SaveChangesAsync();
    }
}
