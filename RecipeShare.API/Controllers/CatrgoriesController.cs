using Microsoft.AspNetCore.Mvc;
using RecipeShare.Application.DTOs.Categories;
using RecipeShare.Application.Interfaces.Services;

namespace RecipeShare.API.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoryResponse>>> GetAll()
    {
        var categories = await _categoryService.GetCategoriesAsync();
        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryResponse>> GetById(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        return Ok(category);
    }
}