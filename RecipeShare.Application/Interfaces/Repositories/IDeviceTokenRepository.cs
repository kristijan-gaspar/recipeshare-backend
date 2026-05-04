using RecipeShare.Domain.Entities;

namespace RecipeShare.Application.Interfaces.Repositories;

public interface IDeviceTokenRepository : IGenericRepository<DeviceToken>
{
    Task<DeviceToken?> GetByTokenAsync(string token);
    Task<List<string>> GetTokensByUserIdAsync(int userId);
    Task DeleteByTokenAsync(string token);
    Task DeleteByUserIdAsync(int userId);
}
