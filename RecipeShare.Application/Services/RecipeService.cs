using Mapster;
using Microsoft.Extensions.Logging;
using RecipeShare.Application.Constants;
using RecipeShare.Application.DTOs.Common;
using RecipeShare.Application.DTOs.Recipes;
using RecipeShare.Application.Enums;
using RecipeShare.Application.Exceptions;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Domain.Entities;

namespace RecipeShare.Application.Services;

public class RecipeService : IRecipeService
{
    private readonly IRecipeRepository _recipeRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IRecipeSocialStatsService _socialStats;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageStorageService _imageStorageService;
    private readonly ILogger<RecipeService> _logger;

    public RecipeService(
        IRecipeRepository recipeRepository,
        ICategoryRepository categoryRepository,
        ITagRepository tagRepository,
        IRecipeSocialStatsService socialStats,
        IUnitOfWork unitOfWork,
        IImageStorageService imageStorageService,
        ILogger<RecipeService> logger)
    {
        _recipeRepository = recipeRepository;
        _categoryRepository = categoryRepository;
        _tagRepository = tagRepository;
        _socialStats = socialStats;
        _unitOfWork = unitOfWork;
        _imageStorageService = imageStorageService;
        _logger = logger;
    }

    public async Task<CursorPagedResponse<RecipeSummaryResponse>> GetRecipesAsync(RecipeQueryParameters parameters, int userId)
    {
        var (items, hasMore) = await _recipeRepository.GetCursorPagedAsync(parameters);

        var recipes = items.ToList();
        var mapped = recipes.Select(r => r.Adapt<RecipeSummaryResponse>()).ToList();
        await _socialStats.ApplyStatsAsync(mapped, userId);

        return new CursorPagedResponse<RecipeSummaryResponse>
        {
            Items = mapped,
            NextCursor = hasMore ? recipes.LastOrDefault()?.Id : null,
            HasMore = hasMore
        };
    }

    public async Task<RecipeDetailResponse> GetRecipeByIdAsync(int id, int userId)
    {
        var recipe = await _recipeRepository.GetDetailedByIdAsync(id);
        if (recipe == null)
            throw new NotFoundException("Recipe not found.");

        var response = recipe.Adapt<RecipeDetailResponse>();
        await _socialStats.ApplyStatsAsync(response, userId);
        return response;
    }

    public async Task<int> CreateAsync(CreateRecipeRequest request, int userId)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category == null)
            throw new NotFoundException("Category not found.");
        if (!category.IsActive)
            throw new BadRequestException("The selected category is not active.");

        var tags = await _tagRepository.GetByIdsAsync(request.TagIds);
        if (tags.Count != request.TagIds.Distinct().Count())
            throw new NotFoundException("One or more tags not found.");
        if (tags.Any(t => !t.IsActive))
            throw new BadRequestException("One or more selected tags are not active.");

        var recipe = new Recipe
        {
            Title = request.Title,
            Description = request.Description,
            PrepTimeMinutes = request.PrepTimeMinutes,
            CookTimeMinutes = request.CookTimeMinutes,
            Servings = request.Servings,
            Difficulty = request.Difficulty,
            CategoryId = request.CategoryId,
            UserId = userId,
            Ingredients = request.Ingredients.Select(i => new Ingredient
            {
                Name = i.Name,
                Quantity = i.Quantity,
                Unit = i.Unit,
                Order = i.Order
            }).ToList(),
            Steps = request.Steps.Select(s => new Step
            {
                Description = s.Description,
                Order = s.Order
            }).ToList(),
            Tags = tags
        };

        await _recipeRepository.AddAsync(recipe);
        await _unitOfWork.SaveChangesAsync();

        return recipe.Id;
    }

    public async Task UpdateAsync(int id, UpdateRecipeRequest request, int userId)
    {
        var recipe = await _recipeRepository.GetDetailedByIdAsync(id);
        if (recipe == null)
            throw new NotFoundException("Recipe not found.");
        if (recipe.UserId != userId)
            throw new ForbiddenException("You can only edit your own recipes.");

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category == null)
            throw new NotFoundException("Category not found.");
        if (!category.IsActive)
            throw new BadRequestException("The selected category is not active.");

        var tags = await _tagRepository.GetByIdsAsync(request.TagIds);
        if (tags.Count != request.TagIds.Distinct().Count())
            throw new NotFoundException("One or more tags not found.");
        if (tags.Any(t => !t.IsActive))
            throw new BadRequestException("One or more selected tags are not active.");

        _recipeRepository.RemoveIngredients(recipe.Ingredients.ToList());
        _recipeRepository.RemoveSteps(recipe.Steps.ToList());

        recipe.Title = request.Title;
        recipe.Description = request.Description;
        recipe.PrepTimeMinutes = request.PrepTimeMinutes;
        recipe.CookTimeMinutes = request.CookTimeMinutes;
        recipe.Servings = request.Servings;
        recipe.Difficulty = request.Difficulty;
        recipe.CategoryId = request.CategoryId;
        recipe.Tags = tags;
        recipe.UpdatedAt = DateTime.UtcNow;

        foreach (var i in request.Ingredients)
            recipe.Ingredients.Add(new Ingredient
            {
                Name = i.Name,
                Quantity = i.Quantity,
                Unit = i.Unit,
                Order = i.Order
            });

        foreach (var s in request.Steps)
            recipe.Steps.Add(new Step
            {
                Description = s.Description,
                Order = s.Order
            });

        _recipeRepository.Update(recipe);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, int userId, bool isAdmin)
    {
        var recipe = await _recipeRepository.GetByIdAsync(id);
        if (recipe == null)
            throw new NotFoundException("Recipe not found.");
        if (recipe.UserId != userId && !isAdmin)
            throw new ForbiddenException("You can only delete your own recipes.");

        if (!string.IsNullOrWhiteSpace(recipe.ImagePublicId))
        {
            try
            {
                await _imageStorageService.DeleteAsync(recipe.ImagePublicId);
            }
            catch (ImageStorageException ex)
            {
                _logger.LogWarning(ex, "Failed to delete recipe image {PublicId} for recipe {RecipeId}.", recipe.ImagePublicId, id);
            }
        }

        _recipeRepository.Delete(recipe);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<string> UploadImageAsync(int id, Stream image, string fileName, int userId)
    {
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension) || !ImageValidation.AllowedExtensions.Contains(extension))
            throw new BadRequestException("Allowed image formats are: .jpg, .jpeg, .png, .webp");

        if (image.Length > ImageValidation.MaxFileSizeBytes)
            throw new BadRequestException("Maximum image size is 5 MB.");

        var recipe = await _recipeRepository.GetByIdAsync(id);
        if (recipe == null)
            throw new NotFoundException("Recipe not found.");
        if (recipe.UserId != userId)
            throw new ForbiddenException("You can only upload images for your own recipes.");

        var oldPublicId = recipe.ImagePublicId;

        var uploaded = await _imageStorageService.UploadAsync(image, fileName, ImageFolder.Recipes);
        recipe.ImageUrl = uploaded.Url;
        recipe.ImagePublicId = uploaded.PublicId;

        _recipeRepository.Update(recipe);
        await _unitOfWork.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(oldPublicId))
        {
            try
            {
                await _imageStorageService.DeleteAsync(oldPublicId);
            }
            catch (ImageStorageException ex)
            {
                _logger.LogWarning(ex, "Failed to delete old recipe image {PublicId} for recipe {RecipeId}.", oldPublicId, id);
            }
        }

        return recipe.ImageUrl;
    }

    public async Task DeleteImageAsync(int id, int userId)
    {
        var recipe = await _recipeRepository.GetByIdAsync(id);
        if (recipe == null)
            throw new NotFoundException("Recipe not found.");
        if (recipe.UserId != userId)
            throw new ForbiddenException("You can only delete images for your own recipes.");

        if (string.IsNullOrWhiteSpace(recipe.ImagePublicId))
            return;

        var oldPublicId = recipe.ImagePublicId;
        recipe.ImageUrl = null;
        recipe.ImagePublicId = null;

        _recipeRepository.Update(recipe);
        await _unitOfWork.SaveChangesAsync();

        try
        {
            await _imageStorageService.DeleteAsync(oldPublicId);
        }
        catch (ImageStorageException ex)
        {
            _logger.LogWarning(ex, "Failed to delete recipe image {PublicId} for recipe {RecipeId}.", oldPublicId, id);
        }
    }
}
