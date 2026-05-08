using RecipeShare.Application.Common;
using RecipeShare.Application.DTOs.Comments;
using RecipeShare.Application.DTOs.Comments.Admin;
using RecipeShare.Application.DTOs.Recipes;
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

        var likeTask = _likeRepository.GetCountsByRecipeIdsAsync(ids);
        var ratingTask = _ratingRepository.GetStatsByRecipeIdsAsync(ids);
        var commentTask = _commentRepository.GetCountsByRecipeIdsAsync(ids);
        await Task.WhenAll(likeTask, ratingTask, commentTask);

        var likeCounts = likeTask.Result;
        var ratingStats = ratingTask.Result;
        var commentCounts = commentTask.Result;

        var items = recipes.Select(r =>
        {
            ratingStats.TryGetValue(r.Id, out var stats);
            return new AdminRecipeListItemResponse
            {
                Id = r.Id,
                Title = r.Title,
                ImageUrl = r.ImageUrl,
                IsFeatured = r.IsFeatured,
                IsDeleted = r.IsDeleted,
                DeletedAt = r.DeletedAt,
                CreatedAt = r.CreatedAt,
                Difficulty = r.Difficulty,
                AuthorId = r.User.Id,
                AuthorUsername = r.User.Username,
                CategoryName = r.Category.Name,
                LikeCount = likeCounts.GetValueOrDefault(r.Id),
                CommentCount = commentCounts.GetValueOrDefault(r.Id),
                AverageRating = stats.Avg,
                RatingCount = stats.Count
            };
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

        var likeTask = _likeRepository.GetCountByRecipeAsync(recipe.Id);
        var ratingTask = _ratingRepository.GetStatsByRecipeAsync(recipe.Id);
        var commentCountTask = _commentRepository.GetCountByRecipeAsync(recipe.Id);
        var commentsTask = _commentRepository.GetAllByRecipeForAdminAsync(recipe.Id);
        await Task.WhenAll(likeTask, ratingTask, commentCountTask, commentsTask);

        var ratingStats = ratingTask.Result;

        return new AdminRecipeDetailResponse
        {
            Id = recipe.Id,
            Title = recipe.Title,
            Description = recipe.Description,
            ImageUrl = recipe.ImageUrl,
            IsFeatured = recipe.IsFeatured,
            IsDeleted = recipe.IsDeleted,
            DeletedAt = recipe.DeletedAt,
            CreatedAt = recipe.CreatedAt,
            UpdatedAt = recipe.UpdatedAt,
            Difficulty = recipe.Difficulty,
            PrepTimeMinutes = recipe.PrepTimeMinutes,
            CookTimeMinutes = recipe.CookTimeMinutes,
            Servings = recipe.Servings,
            CategoryId = recipe.CategoryId,
            CategoryName = recipe.Category.Name,
            Author = new RecipeAuthorResponse
            {
                Id = recipe.User.Id,
                Username = recipe.User.Username,
                ProfileImageUrl = recipe.User.ProfileImageUrl
            },
            Tags = recipe.Tags.Select(t => t.Name).ToList(),
            Ingredients = recipe.Ingredients.OrderBy(i => i.Order).Select(i => new IngredientResponse
            {
                Name = i.Name,
                Quantity = i.Quantity,
                Unit = i.Unit,
                Order = i.Order
            }).ToList(),
            Steps = recipe.Steps.OrderBy(s => s.Order).Select(s => new StepResponse
            {
                Order = s.Order,
                Description = s.Description
            }).ToList(),
            LikeCount = likeTask.Result,
            CommentCount = commentCountTask.Result,
            AverageRating = ratingStats.Avg,
            RatingCount = ratingStats.Count,
            Comments = commentsTask.Result.Select(c => new AdminRecipeCommentItem
            {
                Id = c.Id,
                Content = c.Content,
                IsDeleted = c.IsDeleted,
                DeletedAt = c.DeletedAt,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                Author = new CommentAuthorResponse
                {
                    Id = c.User.Id,
                    Username = c.User.Username,
                    ProfileImageUrl = c.User.ProfileImageUrl
                }
            }).ToList()
        };
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
}
