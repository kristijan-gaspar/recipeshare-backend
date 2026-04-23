using Microsoft.VisualBasic;
using RecipeShare.Application.DTOs.Collections;
using RecipeShare.Application.DTOs.Recipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShare.Application.Interfaces.Services;

public interface ICollectionService
{
    Task<int> CreateAsync(CreateCollectionRequest request, int userId);
    Task DeleteAsync(int collectionId, int userId);
    Task AddRecipeAsync(int collectionId, int recipeId, int userId);
    Task RemoveRecipeAsync(int collectionId, int recipeId,int userId);
    Task<List<CollectionResponse>> GetByUserAsync(int userId);
    Task<List<RecipeSummaryResponse>> GetRecipesAsync(int collectionId, int userId);
}
