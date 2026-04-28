using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeShare.API.Extensions;
using RecipeShare.Application.DTOs.Common;
using RecipeShare.Application.DTOs.Recipes;
using RecipeShare.Application.Interfaces.Services;

namespace RecipeShare.API.Controllers;

[ApiController]
[Route("api/feed")]
[Authorize]
public class FeedController : ControllerBase
{
    private readonly IFeedService _feedService;

    public FeedController(IFeedService feedService)
    {
        _feedService = feedService;
    }

    [HttpGet]
    public async Task<ActionResult<CursorPagedResponse<RecipeSummaryResponse>>> GetFeed(
        [FromQuery] RecipeQueryParameters parameters)
    {
        var userId = User.GetUserId();
        var result = await _feedService.GetFeedAsync(userId, parameters);
        return Ok(result);
    }

    [HttpGet("explore")]
    public async Task<ActionResult<CursorPagedResponse<RecipeSummaryResponse>>> GetExplore(
        [FromQuery] RecipeQueryParameters parameters)
    {
        var userId = User.GetUserId();
        var result = await _feedService.GetExploreAsync(userId, parameters);
        return Ok(result);
    }

    [HttpGet("featured")]
    public async Task<ActionResult<CursorPagedResponse<RecipeSummaryResponse>>> GetFeatured(
        [FromQuery] RecipeQueryParameters parameters)
    {
        var userId = User.GetUserId();
        var result = await _feedService.GetFeaturedAsync(userId, parameters);
        return Ok(result);
    }
}