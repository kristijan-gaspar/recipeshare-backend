using RecipeShare.Application.DTOs.Ratings;
using RecipeShare.Application.Exceptions;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Domain.Entities;

namespace RecipeShare.Application.Services;

public class RatingService : IRatingService
{
    private readonly IRatingRepository _ratingRepo;
    private readonly IRecipeRepository _recipeRepo;
    private readonly IUnitOfWork _unitOfWork;

    public RatingService(IRatingRepository ratingRepo, IRecipeRepository recipeRepo, IUnitOfWork unitOfWork)
    {
        _ratingRepo = ratingRepo;
        _recipeRepo = recipeRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<RatingResponse> RateAsync(int recipeId, int userId, RateRecipeRequest request)
    {
        var recipe = await _recipeRepo.GetByIdAsync(recipeId);
        if (recipe == null || recipe.IsDeleted)
            throw new NotFoundException("Recipe not found");

        if (recipe.UserId == userId)
            throw new BadRequestException("You cannot rate your own recipe");

        var existing = await _ratingRepo.GetByUserAndRecipeAsync(userId, recipeId);

        if (existing != null)
        {
            existing.Value = request.Value;
            await _unitOfWork.SaveChangesAsync();
        }
        else
        {
            await _ratingRepo.AddAsync(new Rating
            {
                UserId = userId,
                RecipeId = recipeId,
                Value = request.Value
            });
            await _unitOfWork.SaveChangesAsync();
        }

        var (avg, count) = await _ratingRepo.GetStatsByRecipeAsync(recipeId);

        return new RatingResponse
        {
            MyRating = request.Value,
            AverageRating = avg,
            RatingCount = count
        };
    }
}
