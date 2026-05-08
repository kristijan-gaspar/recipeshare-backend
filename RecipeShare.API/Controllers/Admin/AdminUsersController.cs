using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeShare.Application.Common;
using RecipeShare.Application.DTOs.Users.Admin;
using RecipeShare.Application.Interfaces.Services;

namespace RecipeShare.API.Controllers.Admin;

[Route("api/admin/users")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;

    public AdminUsersController(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<AdminUserListItemResponse>>> List([FromQuery] AdminUserListQuery query)
    {
        var result = await _adminUserService.GetUsersAsync(query);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AdminUserDetailResponse>> Detail(int id)
    {
        var result = await _adminUserService.GetUserAsync(id);
        return Ok(result);
    }

    [HttpPatch("{id:int}/block")]
    public async Task<IActionResult> ToggleBlock(int id)
    {
        await _adminUserService.ToggleBlockAsync(id);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> SoftDelete(int id)
    {
        await _adminUserService.SoftDeleteAsync(id);
        return NoContent();
    }

    [HttpPost("{id:int}/restore")]
    public async Task<IActionResult> Restore(int id)
    {
        await _adminUserService.RestoreAsync(id);
        return NoContent();
    }
}
