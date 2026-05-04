using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeShare.API.Extensions;
using RecipeShare.Application.DTOs.Likes;
using RecipeShare.Application.Interfaces.Services;

namespace RecipeShare.API.Controllers;

[ApiController]
[Route("api/recipes/{recipeId:int}/like")]
[Authorize]
public class LikesController : ControllerBase
{
    private readonly ILikeService _likeService;

    public LikesController(ILikeService likeService)
    {
        _likeService = likeService;
    }

    [HttpPost]
    public async Task<ActionResult<ToggleLikeResponse>> Toggle(int recipeId)
    {
        var userId = User.GetUserId();
        var username = User.GetUsername();
        var result = await _likeService.ToggleAsync(recipeId, userId, username);
        return Ok(result);
    }
}
