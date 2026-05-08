using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeShare.Application.Interfaces.Services;

namespace RecipeShare.API.Controllers.Admin;

[Route("api/admin/comments")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminCommentsController : ControllerBase
{
    private readonly IAdminCommentService _adminCommentService;

    public AdminCommentsController(IAdminCommentService adminCommentService)
    {
        _adminCommentService = adminCommentService;
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _adminCommentService.SoftDeleteAsync(id);
        return NoContent();
    }
}
