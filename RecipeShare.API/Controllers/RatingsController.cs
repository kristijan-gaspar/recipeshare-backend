using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeShare.API.Extensions;
using RecipeShare.Application.DTOs.Ratings;
using RecipeShare.Application.Interfaces.Services;

namespace RecipeShare.API.Controllers;

[ApiController]
[Route("api/recipes/{recipeId:int}/rating")]
[Authorize]
public class RatingsController : ControllerBase
{
    private readonly IRatingService _ratingService;

    public RatingsController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    [HttpPut]
    public async Task<ActionResult<RatingResponse>> Rate(int recipeId, [FromBody] RateRecipeRequest request)
    {
        var userId = User.GetUserId();
        var result = await _ratingService.RateAsync(recipeId, userId, request);
        return Ok(result);
    }
}
