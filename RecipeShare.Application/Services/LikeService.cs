using Microsoft.Extensions.Logging;
using RecipeShare.Application.DTOs.Likes;
using RecipeShare.Application.Exceptions;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Domain.Entities;

namespace RecipeShare.Application.Services;

public class LikeService : ILikeService
{
    private readonly ILikeRepository _likeRepo;
    private readonly IRecipeRepository _recipeRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<LikeService> _logger;

    public LikeService(ILikeRepository likeRepo, IRecipeRepository recipeRepo, IUnitOfWork unitOfWork, INotificationService notificationService, ILogger<LikeService> logger)
    {
        _likeRepo = likeRepo;
        _recipeRepo = recipeRepo;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<ToggleLikeResponse> ToggleAsync(int recipeId, int userId, string actorUsername)
    {
        var recipe = await _recipeRepo.GetByIdAsync(recipeId);
        if (recipe == null)
            throw new NotFoundException("Recipe not found");

        var existing = await _likeRepo.GetByUserAndRecipeAsync(userId, recipeId);

        if (existing != null)
        {
            _likeRepo.Delete(existing);
            await _unitOfWork.SaveChangesAsync();

            return new ToggleLikeResponse
            {
                IsLiked = false,
                LikeCount = await _likeRepo.GetCountByRecipeAsync(recipeId)
            };
        }

        await _likeRepo.AddAsync(new Like
        {
            UserId = userId,
            RecipeId = recipeId,
            CreatedAt = DateTime.UtcNow
        });
        await _unitOfWork.SaveChangesAsync();

        if (recipe.UserId != userId)
        {
            try { await _notificationService.SendLikeNotificationAsync(recipe.UserId, actorUsername, recipe.Title, recipeId); }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to send like notification for recipe {RecipeId}.", recipeId); }
        }

        return new ToggleLikeResponse
        {
            IsLiked = true,
            LikeCount = await _likeRepo.GetCountByRecipeAsync(recipeId)
        };
    }
}
