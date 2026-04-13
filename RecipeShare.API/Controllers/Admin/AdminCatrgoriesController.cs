using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeShare.Application.DTOs.Categories;
using RecipeShare.Application.Interfaces.Services;

namespace RecipeShare.API.Controllers.Admin;

[ApiController]
[Route("api/admin/categories")]
[Authorize(Roles = "Admin")]
public class AdminCategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public AdminCategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AdminCategoryResponse>>> GetAll([FromQuery] string? searchTerm)
    {
        var categories = await _categoryService.GetAdminCategoriesAsync(searchTerm);
        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AdminCategoryResponse>> GetById(int id)
    {
        var category = await _categoryService.GetAdminCategoryByIdAsync(id);
        return Ok(category);
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateCategoryRequest request)
    {
        var id = await _categoryService.CreateCategoryAsync(request);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreateCategoryRequest request)
    {
        await _categoryService.UpdateCategoryAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _categoryService.DeleteCategoryAsync(id);
        return NoContent();
    }


    [HttpPatch("{id:int}/toggle-IsActive")]
    public async Task<IActionResult> ToggleIsActive(int id)
    {
        await _categoryService.ToggleActiveAsync(id);
        return NoContent();
    }
}
