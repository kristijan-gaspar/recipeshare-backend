using RecipeShare.Application.DTOs.Recipes;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;

namespace RecipeShare.Application.Services;

public class RecipeSocialStatsService : IRecipeSocialStatsService
{
    private readonly ILikeRepository _likeRepository;
    private readonly IRatingRepository _ratingRepository;
    private readonly ICommentRepository _commentRepository;

    public RecipeSocialStatsService(
        ILikeRepository likeRepository,
        IRatingRepository ratingRepository,
        ICommentRepository commentRepository)
    {
        _likeRepository = likeRepository;
        _ratingRepository = ratingRepository;
        _commentRepository = commentRepository;
    }

    public async Task ApplyStatsAsync(List<RecipeSummaryResponse> responses, int userId)
    {
        var recipeIds = responses.Select(r => r.Id).ToList();

        var likeCounts = await _likeRepository.GetCountsByRecipeIdsAsync(recipeIds);
        var likedByMe = await _likeRepository.GetLikedRecipeIdsAsync(userId, recipeIds);
        var ratingStats = await _ratingRepository.GetStatsByRecipeIdsAsync(recipeIds);
        var myRatings = await _ratingRepository.GetMyRatingsByRecipeIdsAsync(userId, recipeIds);
        var commentCounts = await _commentRepository.GetCountsByRecipeIdsAsync(recipeIds);

        foreach (var r in responses)
        {
            r.LikeCount = likeCounts.GetValueOrDefault(r.Id);
            r.IsLikedByMe = likedByMe.Contains(r.Id);
            r.AverageRating = ratingStats.TryGetValue(r.Id, out var s) ? s.Avg : 0;
            r.RatingCount = ratingStats.TryGetValue(r.Id, out var s2) ? s2.Count : 0;
            r.MyRating = myRatings.TryGetValue(r.Id, out var mr) ? mr : null;
            r.CommentCount = commentCounts.GetValueOrDefault(r.Id);
        }
    }

    public async Task ApplyStatsAsync(RecipeDetailResponse response, int userId)
    {
        response.LikeCount = await _likeRepository.GetCountByRecipeAsync(response.Id);
        response.IsLikedByMe = await _likeRepository.GetByUserAndRecipeAsync(userId, response.Id) != null;

        var (avg, ratingCount) = await _ratingRepository.GetStatsByRecipeAsync(response.Id);
        response.AverageRating = avg;
        response.RatingCount = ratingCount;

        var myRating = await _ratingRepository.GetByUserAndRecipeAsync(userId, response.Id);
        response.MyRating = myRating?.Value;

        response.CommentCount = await _commentRepository.GetCountByRecipeAsync(response.Id);
    }
}
