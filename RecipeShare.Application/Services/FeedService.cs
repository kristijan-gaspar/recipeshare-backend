using RecipeShare.Application.DTOs.Common;
using RecipeShare.Application.DTOs.Recipes;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;

namespace RecipeShare.Application.Services;

public class FeedService : IFeedService
{
    private readonly IRecipeRepository _recipeRepo;
    private readonly IFollowRepository _followRepo;
    private readonly IRecipeResponseMapper _recipeResponseMapper;

    public FeedService(
        IRecipeRepository recipeRepository,
        IFollowRepository followRepository,
        IRecipeResponseMapper recipeResponseMapper)
    {
        _recipeRepo = recipeRepository;
        _followRepo = followRepository;
        _recipeResponseMapper = recipeResponseMapper;
    }

    public async Task<CursorPagedResponse<RecipeSummaryResponse>> GetFeedAsync(int userId, RecipeQueryParameters parameters)
    {
        var followingIds = await _followRepo.GetFollowingUserIdsAsync(userId);

        var (items, hasMore) = followingIds.Any()
            ? await _recipeRepo.GetFeedAsync(followingIds, parameters)
            : await _recipeRepo.GetExploreAsync(parameters);

        return await _recipeResponseMapper.ToCursorPagedAsync(items, hasMore, userId);
    }

    public async Task<CursorPagedResponse<RecipeSummaryResponse>> GetExploreAsync(int userId, RecipeQueryParameters parameters)
    {
        var (items, hasMore) = await _recipeRepo.GetExploreAsync(parameters);
        return await _recipeResponseMapper.ToCursorPagedAsync(items, hasMore, userId);
    }

    public async Task<CursorPagedResponse<RecipeSummaryResponse>> GetFeaturedAsync(int userId, RecipeQueryParameters parameters)
    {
        var (items, hasMore) = await _recipeRepo.GetFeaturedAsync(parameters);
        return await _recipeResponseMapper.ToCursorPagedAsync(items, hasMore, userId);
    }
}