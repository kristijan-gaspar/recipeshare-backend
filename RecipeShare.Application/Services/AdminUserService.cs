using Microsoft.Extensions.Logging;
using RecipeShare.Application.Common;
using RecipeShare.Application.DTOs.Users.Admin;
using RecipeShare.Application.Exceptions;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;

namespace RecipeShare.Application.Services;

public class AdminUserService : IAdminUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageStorageService _imageStorageService;
    private readonly IUserStatsService _userStatsService;
    private readonly ILogger<AdminUserService> _logger;

    public AdminUserService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IImageStorageService imageStorageService,
        IUserStatsService userStatsService,
        ILogger<AdminUserService> logger)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _imageStorageService = imageStorageService;
        _userStatsService = userStatsService;
        _logger = logger;
    }

    public async Task<PagedResponse<AdminUserListItemResponse>> GetUsersAsync(AdminUserListQuery query)
    {
        var users = await _userRepository.GetAllPagedAsync(query.Query, query.PageNumber, query.PageSize);
        var totalCount = await _userRepository.CountAllAsync(query.Query);

        var items = users.Select(u => new AdminUserListItemResponse
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            ProfileImageUrl = u.ProfileImageUrl,
            CreatedAt = u.CreatedAt,
            IsBlocked = u.IsBlocked,
            IsDeleted = u.IsDeleted,
            DeletedAt = u.DeletedAt
        }).ToList();

        return new PagedResponse<AdminUserListItemResponse>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            HasNextPage = query.PageNumber * query.PageSize < totalCount
        };
    }

    public async Task<AdminUserDetailResponse> GetUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User not found.");

        var response = new AdminUserDetailResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Bio = user.Bio,
            ProfileImageUrl = user.ProfileImageUrl,
            CreatedAt = user.CreatedAt,
            IsBlocked = user.IsBlocked,
            IsDeleted = user.IsDeleted,
            DeletedAt = user.DeletedAt,
            Role = user.Role
        };

        await _userStatsService.ApplyStatsAsync(response, userId);

        return response;
    }

    public async Task ToggleBlockAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User not found.");

        if (user.IsDeleted)
            throw new BadRequestException("Cannot block a deleted user.");

        user.IsBlocked = !user.IsBlocked;
        _userRepository.Update(user);

        if (user.IsBlocked)
            await _refreshTokenRepository.DeleteAllByUserIdAsync(userId);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RestoreAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User not found.");

        if (!user.IsDeleted)
            throw new BadRequestException("User is not deleted.");

        user.IsDeleted = false;
        user.DeletedAt = null;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || user.IsDeleted)
            throw new NotFoundException("User not found.");

        var profileImagePublicId = user.ProfileImagePublicId;

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;

        _userRepository.Update(user);
        await _refreshTokenRepository.DeleteAllByUserIdAsync(userId);
        await _unitOfWork.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(profileImagePublicId))
        {
            try
            {
                await _imageStorageService.DeleteAsync(profileImagePublicId);
            }
            catch (ImageStorageException ex)
            {
                _logger.LogWarning(ex, "Failed to delete profile image {PublicId} for user {UserId}.", profileImagePublicId, userId);
            }
        }
    }
}
