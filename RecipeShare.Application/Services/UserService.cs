using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RecipeShare.Application.Common;
using RecipeShare.Application.Constants;
using RecipeShare.Application.DTOs.Users;
using RecipeShare.Application.DTOs.Users.Admin;
using RecipeShare.Application.Enums;
using RecipeShare.Application.Exceptions;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Domain.Entities;

namespace RecipeShare.Application.Services;

public class UserService : IUserService
{
    private const string UserNotFoundMessage = "User not found.";
    private const string CurrentPasswordIncorrectMessage = "Current password is incorrect.";

    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageStorageService _imageStorageService;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ILogger<UserService> _logger;
    private readonly IFollowRepository _followRepo;

    public UserService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IImageStorageService imageStorageService,
        IPasswordHasher<User> passwordHasher,
        ILogger<UserService> logger,
        IFollowRepository followRepo)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _imageStorageService = imageStorageService;
        _passwordHasher = passwordHasher;
        _logger = logger;
        _followRepo = followRepo;
    }

    public async Task<UserProfileResponse> GetProfileAsync(int userId, int? currentUserId)
    {
        var user = await GetUserOrThrowAsync(userId);

        var followersCount = await _followRepo.GetFollowerCountAsync(userId);
        var followingCount = await _followRepo.GetFollowingCountAsync(userId);

        bool isFollowedByCurrentUser = false;

        if (currentUserId.HasValue)
        {
            isFollowedByCurrentUser = await _followRepo.ExistsAsync(currentUserId.Value, userId);
        }

        return new UserProfileResponse
        {
            Id = user.Id,
            Username = user.Username,
            ProfileImageUrl = user.ProfileImageUrl,
            Bio = user.Bio,
            RecipeCount = 0,
            FollowerCount = followersCount,
            FollowingCount = followingCount,
            IsFollowedByCurrentUser = isFollowedByCurrentUser
        };
    }

    public async Task UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        var user = await GetUserOrThrowAsync(userId);

        var normalizedUsername = request.Username.ToLowerInvariant();
        if (!string.Equals(user.Username, normalizedUsername, StringComparison.Ordinal))
        {
            if (await _userRepository.UsernameExistsAsync(normalizedUsername))
                throw new BadRequestException("Username is already taken.");

            user.Username = normalizedUsername;
        }

        user.Bio = request.Bio;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await GetUserOrThrowAsync(userId);
        VerifyCurrentPassword(user, request.CurrentPassword);

        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);

        _userRepository.Update(user);
        await _refreshTokenRepository.DeleteAllByUserIdAsync(userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ChangeEmailAsync(int userId, ChangeEmailRequest request)
    {
        var user = await GetUserOrThrowAsync(userId);
        VerifyCurrentPassword(user, request.CurrentPassword);

        var normalizedEmail = request.NewEmail.ToLowerInvariant();
        if (string.Equals(user.Email, normalizedEmail, StringComparison.Ordinal))
            return;

        if (await _userRepository.EmailExistsAsync(normalizedEmail))
            throw new BadRequestException("Email is already taken.");

        user.Email = normalizedEmail;

        _userRepository.Update(user);
        await _refreshTokenRepository.DeleteAllByUserIdAsync(userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateProfileImageAsync(int userId, Stream image, string fileName)
    {
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension) || !ImageValidation.AllowedExtensions.Contains(extension))
            throw new BadRequestException(
                "Allowed image formats are: .jpg, .jpeg, .png, .webp");

        if (image.Length > ImageValidation.MaxFileSizeBytes)
            throw new BadRequestException("Maximum image size is 5 MB.");

        var user = await GetUserOrThrowAsync(userId);

        var oldPublicId = user.ProfileImagePublicId;

        var uploaded = await _imageStorageService.UploadAsync(image, fileName, ImageFolder.Profiles);
        user.ProfileImageUrl = uploaded.Url;
        user.ProfileImagePublicId = uploaded.PublicId;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(oldPublicId))
        {
            try
            {
                await _imageStorageService.DeleteAsync(oldPublicId);
            }
            catch (ImageStorageException ex)
            {
                _logger.LogWarning(ex, "Failed to delete old profile image {PublicId} for user {UserId}.", oldPublicId, userId);
            }
        }
    }

    public async Task DeleteProfileImageAsync(int userId)
    {
        var user = await GetUserOrThrowAsync(userId);

        if (string.IsNullOrWhiteSpace(user.ProfileImagePublicId))
            return;

        var oldPublicId = user.ProfileImagePublicId;
        user.ProfileImageUrl = null;
        user.ProfileImagePublicId = null;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        try
        {
            await _imageStorageService.DeleteAsync(oldPublicId);
        }
        catch (ImageStorageException ex)
        {
            _logger.LogWarning(ex, "Failed to delete profile image {PublicId} for user {UserId}.", oldPublicId, userId);
        }
    }

    public async Task SoftDeleteAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || user.IsDeleted)
            throw new NotFoundException(UserNotFoundMessage);

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;

        _userRepository.Update(user);
        await _refreshTokenRepository.DeleteAllByUserIdAsync(userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAccountAsync(int userId, DeleteAccountRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException(UserNotFoundMessage);

        VerifyCurrentPassword(user, request.Password);

        var profileImagePublicId = user.ProfileImagePublicId;

        _userRepository.Delete(user);
        await _unitOfWork.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(profileImagePublicId))
        {
            try { await _imageStorageService.DeleteAsync(profileImagePublicId); }
            catch (ImageStorageException ex) { _logger.LogWarning(ex, "Failed to delete profile image {PublicId} for user {UserId}.", profileImagePublicId, userId); }
        }
    }

    private async Task<User> GetUserOrThrowAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || user.IsDeleted)
            throw new NotFoundException(UserNotFoundMessage);
        return user;
    }

    private void VerifyCurrentPassword(User user, string currentPassword)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedException(CurrentPasswordIncorrectMessage);
    }

    public async Task<PagedResponse<UserSearchResponse>> SearchUsersAsync (string query, int pageNumber, int pageSize)
    {
        var users = await _userRepository.SearchByUsername(query, pageNumber, pageSize);
        var totalCount = await _userRepository.CountByUsernameAsync(query);

        var mappedUsers = users.Select(u => new UserSearchResponse
        {
            Id = u.Id,
            Username = u.Username,
            ProfileImageUrl = u.ProfileImageUrl
        }).ToList();

        return new PagedResponse<UserSearchResponse>
        {
            Items = mappedUsers,
            TotalCount = await _userRepository.CountByUsernameAsync(query),
            PageNumber = pageNumber,
            PageSize = pageSize,
            HasNextPage = pageNumber * pageSize < totalCount
        };
    }


}
