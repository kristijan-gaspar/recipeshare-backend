using RecipeShare.Application.DTOs.Likes;

namespace RecipeShare.Application.Interfaces.Services;

public interface ILikeService
{
    Task<ToggleLikeResponse> ToggleAsync(int recipeId, int userId, string actorUsername);
}
