using RecipeShare.Application.DTOs.Users;

namespace RecipeShare.Application.Interfaces.Services;

public interface IUserService
{
    Task<UserProfileResponse> GetProfileAsync(int userId, int? currentUserId);
    Task UpdateProfileAsync(int userId, UpdateProfileRequest request, Stream? image, string? imageName);
}
