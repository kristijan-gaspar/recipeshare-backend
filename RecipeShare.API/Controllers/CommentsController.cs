using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeShare.API.Extensions;
using RecipeShare.Application.DTOs.Comments;
using RecipeShare.Application.DTOs.Common;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Domain.Enums;

namespace RecipeShare.API.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet("recipes/{recipeId:int}/comments")]
    public async Task<ActionResult<CursorPagedResponse<CommentResponse>>> GetPaged(
        int recipeId, [FromQuery] CommentQueryParameters parameters)
    {
        var result = await _commentService.GetPagedAsync(recipeId, parameters);
        return Ok(result);
    }

    [HttpPost("recipes/{recipeId:int}/comments")]
    public async Task<ActionResult<CommentResponse>> Create(
        int recipeId, [FromBody] CommentRequest request)
    {
        var userId = User.GetUserId();
        var username = User.GetUsername();
        var result = await _commentService.CreateAsync(recipeId, userId, request, username);
        return CreatedAtAction(nameof(GetPaged), new { recipeId }, result);
    }

    [HttpPut("comments/{id:int}")]
    public async Task<ActionResult<CommentResponse>> Update(
        int id, [FromBody] CommentRequest request)
    {
        var userId = User.GetUserId();
        var result = await _commentService.UpdateAsync(id, userId, request);
        return Ok(result);
    }

    [HttpDelete("comments/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.GetUserId();
        var isAdmin = User.GetUserRole() == UserRole.Admin;
        await _commentService.DeleteAsync(id, userId, isAdmin);
        return NoContent();
    }
}
