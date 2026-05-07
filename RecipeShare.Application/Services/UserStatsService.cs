using RecipeShare.Application.DTOs.Users.Admin;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;

namespace RecipeShare.Application.Services;

public class UserStatsService : IUserStatsService
{
    private readonly IRecipeRepository _recipeRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly ILikeRepository _likeRepository;
    private readonly IRatingRepository _ratingRepository;
    private readonly IFollowRepository _followRepository;

    public UserStatsService(
        IRecipeRepository recipeRepository,
        ICommentRepository commentRepository,
        ILikeRepository likeRepository,
        IRatingRepository ratingRepository,
        IFollowRepository followRepository)
    {
        _recipeRepository = recipeRepository;
        _commentRepository = commentRepository;
        _likeRepository = likeRepository;
        _ratingRepository = ratingRepository;
        _followRepository = followRepository;
    }

    public async Task ApplyStatsAsync(AdminUserDetailResponse response, int userId)
    {
        var recipeCountTask = _recipeRepository.GetCountByUserAsync(userId);
        var commentCountTask = _commentRepository.GetCountByUserAsync(userId);
        var likeCountTask = _likeRepository.GetCountByUserAsync(userId);
        var ratingCountTask = _ratingRepository.GetCountByUserAsync(userId);
        var followerCountTask = _followRepository.GetFollowerCountAsync(userId);
        var followingCountTask = _followRepository.GetFollowingCountAsync(userId);
        var recentRecipesTask = _recipeRepository.GetRecentByUserAsync(userId, 5);
        var recentCommentsTask = _commentRepository.GetRecentByUserAsync(userId, 5);

        await Task.WhenAll(
            recipeCountTask, commentCountTask, likeCountTask, ratingCountTask,
            followerCountTask, followingCountTask, recentRecipesTask, recentCommentsTask);

        response.RecipeCount = recipeCountTask.Result;
        response.CommentCount = commentCountTask.Result;
        response.LikeCount = likeCountTask.Result;
        response.RatingCount = ratingCountTask.Result;
        response.FollowerCount = followerCountTask.Result;
        response.FollowingCount = followingCountTask.Result;

        response.RecentRecipes = recentRecipesTask.Result.Select(r => new AdminUserRecipeItem
        {
            Id = r.Id,
            Title = r.Title,
            ImageUrl = r.ImageUrl,
            CreatedAt = r.CreatedAt,
            IsFeatured = r.IsFeatured,
            IsDeleted = r.IsDeleted,
            DeletedAt = r.DeletedAt
        }).ToList();

        response.RecentComments = recentCommentsTask.Result.Select(c => new AdminUserCommentItem
        {
            Id = c.Id,
            Content = c.Content,
            CreatedAt = c.CreatedAt,
            RecipeId = c.RecipeId,
            RecipeTitle = c.Recipe!.Title,
            IsDeleted = c.IsDeleted,
            DeletedAt = c.DeletedAt
        }).ToList();
    }
}
