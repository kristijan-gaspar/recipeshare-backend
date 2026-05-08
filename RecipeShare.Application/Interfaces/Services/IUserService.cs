using RecipeShare.Application.Common;
using RecipeShare.Application.DTOs.Users;

namespace RecipeShare.Application.Interfaces.Services;

public interface IUserService
{
    Task<UserProfileResponse> GetProfileAsync(int userId, int? currentUserId);
    Task UpdateProfileAsync(int userId, UpdateProfileRequest request);
    Task ChangePasswordAsync(int userId, ChangePasswordRequest request);
    Task ChangeEmailAsync(int userId, ChangeEmailRequest request);
    Task UpdateProfileImageAsync(int userId, Stream image, string fileName);
    Task DeleteProfileImageAsync(int userId);
    Task<PagedResponse<UserSearchResponse>> SearchUsersAsync(string query, int pageNumber, int pageSize);
    Task DeleteAccountAsync(int userId, DeleteAccountRequest request);
}
