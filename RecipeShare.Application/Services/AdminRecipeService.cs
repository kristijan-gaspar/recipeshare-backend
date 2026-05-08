using Mapster;
using RecipeShare.Application.Common;
using RecipeShare.Application.DTOs.Comments.Admin;
using RecipeShare.Application.DTOs.Recipes.Admin;
using RecipeShare.Application.Exceptions;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;

namespace RecipeShare.Application.Services;

public class AdminRecipeService : IAdminRecipeService
{
    private readonly IRecipeRepository _recipeRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly ILikeRepository _likeRepository;
    private readonly IRatingRepository _ratingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AdminRecipeService(
        IRecipeRepository recipeRepository,
        ICommentRepository commentRepository,
        ILikeRepository likeRepository,
        IRatingRepository ratingRepository,
        IUnitOfWork unitOfWork)
    {
        _recipeRepository = recipeRepository;
        _commentRepository = commentRepository;
        _likeRepository = likeRepository;
        _ratingRepository = ratingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResponse<AdminRecipeListItemResponse>> GetRecipesAsync(AdminRecipeListQuery query)
    {
        var recipes = (await _recipeRepository.GetAllPagedAsync(query)).ToList();
        var totalCount = await _recipeRepository.CountAllAsync(query);

        var ids = recipes.Select(r => r.Id).ToList();

        var likeCounts = await _likeRepository.GetCountsByRecipeIdsAsync(ids);
        var ratingStats = await _ratingRepository.GetStatsByRecipeIdsAsync(ids);
        var commentCounts = await _commentRepository.GetCountsByRecipeIdsAsync(ids);

        var items = recipes.Select(r =>
        {
            ratingStats.TryGetValue(r.Id, out var stats);
            var item = r.Adapt<AdminRecipeListItemResponse>();
            item.LikeCount = likeCounts.GetValueOrDefault(r.Id);
            item.CommentCount = commentCounts.GetValueOrDefault(r.Id);
            item.AverageRating = stats.Avg;
            item.RatingCount = stats.Count;
            return item;
        }).ToList();

        return new PagedResponse<AdminRecipeListItemResponse>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            HasNextPage = query.PageNumber * query.PageSize < totalCount
        };
    }

    public async Task<AdminRecipeDetailResponse> GetRecipeAsync(int recipeId)
    {
        var recipe = await _recipeRepository.GetDetailedByIdForAdminAsync(recipeId);
        if (recipe == null)
            throw new NotFoundException("Recipe not found.");

        var likeCount = await _likeRepository.GetCountByRecipeAsync(recipe.Id);
        var ratingStats = await _ratingRepository.GetStatsByRecipeAsync(recipe.Id);
        var commentCount = await _commentRepository.GetCountByRecipeAsync(recipe.Id);
        var comments = await _commentRepository.GetAllByRecipeForAdminAsync(recipe.Id);

        var response = recipe.Adapt<AdminRecipeDetailResponse>();
        response.LikeCount = likeCount;
        response.CommentCount = commentCount;
        response.AverageRating = ratingStats.Avg;
        response.RatingCount = ratingStats.Count;
        response.Comments = comments.Adapt<List<AdminRecipeCommentItem>>();
        return response;
    }

    public async Task SoftDeleteAsync(int recipeId)
    {
        var recipe = await _recipeRepository.GetByIdAsync(recipeId);
        if (recipe == null || recipe.IsDeleted)
            throw new NotFoundException("Recipe not found.");

        recipe.IsDeleted = true;
        recipe.DeletedAt = DateTime.UtcNow;

        _recipeRepository.Update(recipe);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RestoreAsync(int recipeId)
    {
        var recipe = await _recipeRepository.GetByIdAsync(recipeId);
        if (recipe == null)
            throw new NotFoundException("Recipe not found.");

        if (!recipe.IsDeleted)
            throw new BadRequestException("Recipe is not deleted.");

        recipe.IsDeleted = false;
        recipe.DeletedAt = null;

        _recipeRepository.Update(recipe);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ToggleFeaturedAsync(int recipeId)
    {
        var recipe = await _recipeRepository.GetByIdAsync(recipeId);
        if (recipe == null || recipe.IsDeleted)
            throw new NotFoundException("Recipe not found.");

        recipe.IsFeatured = !recipe.IsFeatured;
        recipe.UpdatedAt = DateTime.UtcNow;

        _recipeRepository.Update(recipe);
        await _unitOfWork.SaveChangesAsync();
    }
}
