using RecipeShare.Application.DTOs.Users;
using RecipeShare.Application.Exceptions;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;

namespace RecipeShare.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageStorageService _imageStorageService;

    public UserService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IImageStorageService imageStorageService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _imageStorageService = imageStorageService;
    }

    public async Task<UserProfileResponse> GetProfileAsync(int userId, int? currentUserId)
    {
        var user = await _userRepository.GetProfileByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User not found.");

        return new UserProfileResponse
        {
            Id = user.Id,
            Username = user.Username,
            ProfileImageUrl = user.ProfileImageUrl,
            Bio = user.Bio,
            RecipeCount = 0,
            FollowerCount = 0,
            FollowingCount = 0,
            IsFollowedByCurrentUser = false
        };
    }

    public async Task UpdateProfileAsync(int userId, UpdateProfileRequest request, Stream? image, string? imageName)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User not found");

        user.Bio = request.Bio;

        if (image != null && !string.IsNullOrWhiteSpace(imageName))
        {
            if (!string.IsNullOrWhiteSpace(user.ProfileImageUrl))
            {
                await _imageStorageService.DeleteAsync(user.ProfileImageUrl);
            }

            user.ProfileImageUrl = await _imageStorageService.UploadAsync(image, imageName);
        }

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();
    }
}