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

    public LikeService(ILikeRepository likeRepo, IRecipeRepository recipeRepo, IUnitOfWork unitOfWork)
    {
        _likeRepo = likeRepo;
        _recipeRepo = recipeRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<ToggleLikeResponse> ToggleAsync(int recipeId, int userId)
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

        return new ToggleLikeResponse
        {
            IsLiked = true,
            LikeCount = await _likeRepo.GetCountByRecipeAsync(recipeId)
        };
    }
}
