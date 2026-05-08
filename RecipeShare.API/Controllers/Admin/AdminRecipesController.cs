using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeShare.Application.Common;
using RecipeShare.Application.DTOs.Recipes.Admin;
using RecipeShare.Application.Interfaces.Services;

namespace RecipeShare.API.Controllers.Admin;

[Route("api/admin/recipes")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminRecipesController : ControllerBase
{
    private readonly IAdminRecipeService _adminRecipeService;

    public AdminRecipesController(IAdminRecipeService adminRecipeService)
    {
        _adminRecipeService = adminRecipeService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<AdminRecipeListItemResponse>>> List([FromQuery] AdminRecipeListQuery query)
    {
        var result = await _adminRecipeService.GetRecipesAsync(query);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AdminRecipeDetailResponse>> Detail(int id)
    {
        var result = await _adminRecipeService.GetRecipeAsync(id);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _adminRecipeService.SoftDeleteAsync(id);
        return NoContent();
    }

    [HttpPost("{id:int}/restore")]
    public async Task<IActionResult> Restore(int id)
    {
        await _adminRecipeService.RestoreAsync(id);
        return NoContent();
    }
}
