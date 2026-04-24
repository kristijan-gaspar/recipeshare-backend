using RecipeShare.Application.DTOs.Ratings;

namespace RecipeShare.Application.Interfaces.Services;

public interface IRatingService
{
    Task<RatingResponse> RateAsync(int recipeId, int userId, RateRecipeRequest request);
}
