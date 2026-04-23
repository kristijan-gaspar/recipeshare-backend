using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeShare.API.Extensions;
using RecipeShare.Application.DTOs.Collections;
using RecipeShare.Application.DTOs.Recipes;
using RecipeShare.Application.Interfaces.Services;

namespace RecipeShare.API.Controllers;

[Route("api/collections")]
[ApiController]
[Authorize]
public class CollectionsController : ControllerBase
{
    private readonly ICollectionService _collectionService;

    public CollectionsController(ICollectionService collectionService)
    {
        _collectionService = collectionService;
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] CreateCollectionRequest request)
    {
        var userId = User.GetUserId();
        var id = await _collectionService.CreateAsync(request, userId);
        return StatusCode(StatusCodes.Status201Created, id);
    }

    [HttpDelete("{collectionId:int}")]
    public async Task<IActionResult> Delete(int collectionId)
    {
        var userId = User.GetUserId();
        await _collectionService.DeleteAsync(collectionId, userId);
        return NoContent();
    }

    [HttpPost("{collectionId:int}/recipes/{recipeId:int}")]
    public async Task<ActionResult<MessageResponse>> AddRecipe(int collectionId, int recipeId)
    {
        var userId = User.GetUserId();
        await _collectionService.AddRecipeAsync(collectionId, recipeId, userId);

        return Ok(new MessageResponse
        {
            Message = "Added successfully"
        });
    }

    [HttpDelete("{collectionId:int}/recipes/{recipeId:int}")]
    public async Task<ActionResult<MessageResponse>> RemoveRecipe(int collectionId, int recipeId)
    {
        var userId = User.GetUserId();
        await _collectionService.RemoveRecipeAsync(collectionId, recipeId, userId);

        return Ok(new MessageResponse
        {
            Message = "Removed successfully"
        });
    }

    [HttpGet]
    public async Task<ActionResult<List<CollectionResponse>>> GetMyCollections()
    {
        var userId = User.GetUserId();
        var collections = await _collectionService.GetByUserAsync(userId);
        return Ok(collections);
    }

    [HttpGet("{id:int}/recipes")]
    public async Task<ActionResult<List<RecipeSummaryResponse>>> GetRecipes(int id)
    {
        var userId = User.GetUserId();
        var result = await _collectionService.GetRecipesAsync(id, userId);
        return Ok(result);
    }

}
