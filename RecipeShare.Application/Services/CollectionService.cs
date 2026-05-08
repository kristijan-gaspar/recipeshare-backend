using RecipeShare.Application.DTOs.Collections;
using RecipeShare.Application.DTOs.Recipes;
using RecipeShare.Application.Exceptions;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Domain.Entities;

namespace RecipeShare.Application.Services;

public class CollectionService : ICollectionService
{
    private readonly ICollectionRepository _collectionRepo;
    private readonly IRecipeRepository _recipeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CollectionService(
        ICollectionRepository collectionRepo,
        IRecipeRepository recipeRepository,
        IUnitOfWork unitOfWork)
    {
        _collectionRepo = collectionRepo;
        _recipeRepository = recipeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> CreateAsync(CreateCollectionRequest request, int userId)
    {
        var normalizedName = request.Name.Trim();

        if (await _collectionRepo.NameExistsForUserAsync(userId, normalizedName))
            throw new BadRequestException("You already have a collection with this name");

        var collection = new Collection
        {
            Name = normalizedName,
            UserId = userId
        };

        await _collectionRepo.AddAsync(collection);
        await _unitOfWork.SaveChangesAsync();

        return collection.Id;
    }

    public async Task DeleteAsync(int collectionId, int userId)
    {
        var collection = await _collectionRepo.GetByIdAndUserIdAsync(collectionId, userId);
        if (collection == null)
            throw new NotFoundException("Collection not found.");

        _collectionRepo.Delete(collection);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AddRecipeAsync(int collectionId, int recipeId, int userId)
    {
        var collection = await _collectionRepo.GetByIdAndUserIdAsync(collectionId, userId);
        if (collection == null)
            throw new NotFoundException("Collection not found");

        var recipe = await _recipeRepository.GetByIdAsync(recipeId);
        if (recipe == null || recipe.IsDeleted)
            throw new NotFoundException("Recipe not found");

        var alreadyAdded = collection.CollectionRecipes.Any(cr => cr.RecipeId == recipeId);
        if (alreadyAdded)
            throw new BadRequestException("Recipe already in collection");

        collection.CollectionRecipes.Add(new CollectionRecipe
        {
            CollectionId = collectionId,
            RecipeId = recipeId
        });

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveRecipeAsync(int collectionId, int recipeId, int userId)
    {
        var collection = await _collectionRepo.GetByIdAndUserIdAsync(collectionId, userId);
        if (collection == null)
            throw new NotFoundException("Collection not found");

        var collectionRecipe = collection.CollectionRecipes.FirstOrDefault(cr => cr.RecipeId == recipeId);
        if (collectionRecipe == null)
            throw new NotFoundException("Recipe not in collection");

        collection.CollectionRecipes.Remove(collectionRecipe);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<CollectionResponse>> GetByUserAsync(int userId)
    {
        var collections = await _collectionRepo.GetByUserIdAsync(userId);

        return collections.Select(c => new CollectionResponse
        {
            Id = c.Id,
            Name = c.Name,
            CreatedAt = c.CreatedAt,
            RecipeCount = c.CollectionRecipes.Count
        }).ToList();
    }

    public async Task<List<RecipeSummaryResponse>> GetRecipesAsync(int collectionId, int userId)
    {
        var collection = await _collectionRepo.GetWithRecipesAsync(collectionId);

        if (collection == null)
            throw new NotFoundException("Collection not found.");

        if (collection.UserId != userId)
            throw new ForbiddenException("You do not have access to this collection.");

        return collection.CollectionRecipes
            .Select(cr => cr.Recipe)
            .Select(r => new RecipeSummaryResponse
            {
                Id = r.Id,
                Title = r.Title,
                ImageUrl = r.ImageUrl,
                Author = new RecipeAuthorResponse
                {
                    Id = r.User.Id,
                    Username = r.User.Username,
                    ProfileImageUrl = r.User.ProfileImageUrl
                },
                CategoryName = r.Category.Name,
                Tags = r.Tags.Select(rt => rt.Name).ToList(),
                Difficulty = r.Difficulty,
                PrepTimeMinutes = r.PrepTimeMinutes,
                CookTimeMinutes = r.CookTimeMinutes,
                IsFeatured = r.IsFeatured,
                CreatedAt = r.CreatedAt
            })
            .ToList();
    }
}