using Microsoft.Extensions.Logging;
using RecipeShare.Application.Exceptions;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Domain.Entities;

namespace RecipeShare.Application.Services;

public class FollowService : IFollowService
{
    private readonly IFollowRepository _followRepo;
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<FollowService> _logger;

    public FollowService(IFollowRepository followRepo, IUserRepository userRepo, IUnitOfWork unitOfWork, INotificationService notificationService, ILogger<FollowService> logger)
    {
        _followRepo = followRepo;
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<bool> ToggleFollowAsync(int targetUserId, int currentUserId, string followerUsername)
    {
        if (targetUserId == currentUserId)
            throw new BadRequestException("You can't follow yourself");

        var targetUser = await _userRepo.GetByIdAsync(targetUserId);
        if (targetUser == null)
            throw new NotFoundException("User not found");

        var existingFollow = await _followRepo.GetByUsersAsync(currentUserId, targetUserId);

        if (existingFollow != null)
        {
            _followRepo.Delete(existingFollow);
            await _unitOfWork.SaveChangesAsync();
            return false;
        }

        await _followRepo.AddAsync(new Follow
        {
            FollowerId = currentUserId,
            FollowedId = targetUserId,
            CreatedAt = DateTime.UtcNow
        });
        await _unitOfWork.SaveChangesAsync();

        try { await _notificationService.SendFollowNotificationAsync(targetUserId, followerUsername, currentUserId); }
        catch (Exception ex) { _logger.LogWarning(ex, "Failed to send follow notification for user {TargetUserId}.", targetUserId); }

        return true;
    }
}
