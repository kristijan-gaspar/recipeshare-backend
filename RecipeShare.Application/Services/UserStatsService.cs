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
        var recipeCount = await _recipeRepository.GetCountByUserAsync(userId);
        var commentCount = await _commentRepository.GetCountByUserAsync(userId);
        var likeCount = await _likeRepository.GetCountByUserAsync(userId);
        var ratingCount = await _ratingRepository.GetCountByUserAsync(userId);
        var followerCount = await _followRepository.GetFollowerCountAsync(userId);
        var followingCount = await _followRepository.GetFollowingCountAsync(userId);
        var recentRecipes = await _recipeRepository.GetRecentByUserAsync(userId, 5);
        var recentComments = await _commentRepository.GetRecentByUserAsync(userId, 5);

        response.RecipeCount = recipeCount;
        response.CommentCount = commentCount;
        response.LikeCount = likeCount;
        response.RatingCount = ratingCount;
        response.FollowerCount = followerCount;
        response.FollowingCount = followingCount;

        response.RecentRecipes = recentRecipes.Select(r => new AdminUserRecipeItem
        {
            Id = r.Id,
            Title = r.Title,
            ImageUrl = r.ImageUrl,
            CreatedAt = r.CreatedAt,
            IsFeatured = r.IsFeatured,
            IsDeleted = r.IsDeleted,
            DeletedAt = r.DeletedAt
        }).ToList();

        response.RecentComments = recentComments.Select(c => new AdminUserCommentItem
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
