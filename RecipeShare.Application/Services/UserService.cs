using Microsoft.AspNetCore.Identity;
using RecipeShare.Application.Constants;
using RecipeShare.Application.DTOs.Users;
using RecipeShare.Application.Exceptions;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Domain.Entities;

namespace RecipeShare.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageStorageService _imageStorageService;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IImageStorageService imageStorageService,
        IPasswordHasher<User> passwordHasher)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _imageStorageService = imageStorageService;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserProfileResponse> GetProfileAsync(int userId, int? currentUserId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
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

    public async Task UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User not found.");

        if (!string.Equals(user.Username, request.Username, StringComparison.Ordinal))
        {
            if (await _userRepository.UsernameExistsAsync(request.Username))
                throw new BadRequestException("Username is already taken.");

            user.Username = request.Username;
        }

        user.Bio = request.Bio;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User not found.");

        var verification = _passwordHasher.VerifyHashedPassword(
            user, user.PasswordHash, request.CurrentPassword);

        if (verification == PasswordVerificationResult.Failed)
            throw new UnauthorizedException("Current password is incorrect.");

        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);

        _userRepository.Update(user);
        await _refreshTokenRepository.DeleteAllByUserIdAsync(userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ChangeEmailAsync(int userId, ChangeEmailRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User not found.");

        var verification = _passwordHasher.VerifyHashedPassword(
            user, user.PasswordHash, request.CurrentPassword);

        if (verification == PasswordVerificationResult.Failed)
            throw new UnauthorizedException("Current password is incorrect.");

        if (string.Equals(user.Email, request.NewEmail, StringComparison.OrdinalIgnoreCase))
            return;

        if (await _userRepository.EmailExistsAsync(request.NewEmail))
            throw new BadRequestException("Email is already taken.");

        user.Email = request.NewEmail;

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

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User not found.");

        var oldImageUrl = user.ProfileImageUrl;

        user.ProfileImageUrl = await _imageStorageService.UploadAsync(image, fileName);

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(oldImageUrl))
        {
            try
            {
                await _imageStorageService.DeleteAsync(oldImageUrl);
            }
            catch
            {
                // Best-effort cleanup. An orphan file is recoverable;
                // failing the request because of cleanup is not.
            }
        }
    }

    public async Task DeleteProfileImageAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User not found.");

        if (string.IsNullOrWhiteSpace(user.ProfileImageUrl))
            return;

        var oldImageUrl = user.ProfileImageUrl;
        user.ProfileImageUrl = null;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        try
        {
            await _imageStorageService.DeleteAsync(oldImageUrl);
        }
        catch
        {
            // Best-effort cleanup. DB is the source of truth;
            // an orphan file is recoverable.
        }
    }
}
