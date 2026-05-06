using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeShare.API.Extensions;
using RecipeShare.Application.DTOs.Common;
using RecipeShare.Application.DTOs.Recipes;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Domain.Enums;

namespace RecipeShare.API.Controllers;

[ApiController]
[Route("api/recipes")]
[Authorize]
public class RecipesController : ControllerBase
{
    private readonly IRecipeService _recipeService;

    public RecipesController(IRecipeService recipeService)
    {
        _recipeService = recipeService;
    }

    [HttpGet]
    public async Task<ActionResult<CursorPagedResponse<RecipeSummaryResponse>>> GetAll(
        [FromQuery] RecipeQueryParameters parameters)
    {
        var userId = User.GetUserId();
        var result = await _recipeService.GetRecipesAsync(parameters, userId);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RecipeDetailResponse>> GetById(int id)
    {
        var userId = User.GetUserId();
        var recipe = await _recipeService.GetRecipeByIdAsync(id, userId);
        return Ok(recipe);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRecipeRequest request)
    {
        var userId = User.GetUserId();
        var id = await _recipeService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRecipeRequest request)
    {
        var userId = User.GetUserId();
        await _recipeService.UpdateAsync(id, request, userId);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.GetUserId();
        var isAdmin = User.GetUserRole() == UserRole.Admin;
        await _recipeService.DeleteAsync(id, userId, isAdmin);
        return NoContent();
    }

    [HttpDelete("{id:int}/soft")]
    public async Task<IActionResult> SoftDelete(int id)
    {
        var userId = User.GetUserId();
        var isAdmin = User.GetUserRole() == UserRole.Admin;
        await _recipeService.SoftDeleteAsync(id, userId, isAdmin);
        return NoContent();
    }

    [HttpPut("{id:int}/image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImage(int id, IFormFile image)
    {
        if (image is null || image.Length == 0)
            return BadRequest("Image file is required.");

        var userId = User.GetUserId();
        await using var stream = image.OpenReadStream();
        var url = await _recipeService.UploadImageAsync(id, stream, image.FileName, userId);
        return Ok(new { url });
    }

    [HttpDelete("{id:int}/image")]
    public async Task<IActionResult> DeleteImage(int id)
    {
        var userId = User.GetUserId();
        await _recipeService.DeleteImageAsync(id, userId);
        return NoContent();
    }

    [HttpPatch("{id}/feature")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Feature(int id)
    {
        var userId = User.GetUserId();
        var isAdmin = User.GetUserRole() == UserRole.Admin;
        await _recipeService.ToggleFeaturedAsync(id, true, userId, isAdmin);

        return NoContent();
    }

    [HttpPatch("{id}/unfeature")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Unfeature(int id)
    {
        var userId = User.GetUserId();
        var isAdmin = User.GetUserRole() == UserRole.Admin;
        await _recipeService.ToggleFeaturedAsync(id, false, userId, isAdmin);

        return NoContent();
    }
}