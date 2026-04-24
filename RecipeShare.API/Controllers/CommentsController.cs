using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeShare.API.Extensions;
using RecipeShare.Application.DTOs.Comments;
using RecipeShare.Application.DTOs.Common;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Domain.Enums;

namespace RecipeShare.API.Controllers;

[ApiController]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet("api/recipes/{recipeId:int}/comments")]
    public async Task<ActionResult<CursorPagedResponse<CommentResponse>>> GetPaged(
        int recipeId, [FromQuery] CommentQueryParameters parameters)
    {
        var result = await _commentService.GetPagedAsync(recipeId, parameters);
        return Ok(result);
    }

    [HttpPost("api/recipes/{recipeId:int}/comments")]
    public async Task<ActionResult<CommentResponse>> Create(
        int recipeId, [FromBody] CommentRequest request)
    {
        var userId = User.GetUserId();
        var result = await _commentService.CreateAsync(recipeId, userId, request);
        return CreatedAtAction(nameof(GetPaged), new { recipeId }, result);
    }

    [HttpPut("api/comments/{id:int}")]
    public async Task<ActionResult<CommentResponse>> Update(
        int id, [FromBody] CommentRequest request)
    {
        var userId = User.GetUserId();
        var result = await _commentService.UpdateAsync(id, userId, request);
        return Ok(result);
    }

    [HttpDelete("api/comments/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.GetUserId();
        var isAdmin = User.GetUserRole() == UserRole.Admin;
        await _commentService.DeleteAsync(id, userId, isAdmin);
        return NoContent();
    }
}
