using Microsoft.AspNetCore.Mvc;
using RecipeShare.Application.DTOs.Tags;
using RecipeShare.Application.Interfaces.Services;

namespace RecipeShare.API.Controllers;

[ApiController]
[Route("api/tags")]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagsController(ITagService tagService)
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
}