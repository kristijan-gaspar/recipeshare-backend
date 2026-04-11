using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeShare.Application.DTOs.Tags;
using RecipeShare.Application.Interfaces.Services;

namespace RecipeShare.API.Controllers.Admin;

[ApiController]
[Route("api/admin/tags")]
[Authorize(Roles = "Admin")]
public class AdminTagsController : ControllerBase
{
    private readonly IAdminTagService _tagService;

    public AdminTagsController(IAdminTagService tagService)
    {
        _tagService = tagService;
    }

    [HttpGet]
    public async Task<ActionResult<List<TagResponse>>> GetAll()
    {
        var tags = await _tagService.GetTagsAsync();
        return Ok(tags);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TagResponse>> GetById(int id)
    {
        var tag = await _tagService.GetTagByIdAsync(id);
        return Ok(tag);
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateTagRequest request)
    {
        var id = await _tagService.CreateTagAsync(request);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreateTagRequest request)
    {
        await _tagService.UpdateTagAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _tagService.DeleteTagAsync(id);
        return NoContent();
    }
}