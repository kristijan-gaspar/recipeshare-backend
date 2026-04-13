using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecipeShare.Application.Common;
using RecipeShare.Application.DTOs.Users;
using RecipeShare.Application.Exceptions;
using RecipeShare.Application.Interfaces.Services;

namespace RecipeShare.API.Controllers.Admin;

[Route("api/admin/users")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly IUserService _userService;

    public AdminUsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedResponse<AdminUserSearchResponse>>> SearchUsers(
    [FromQuery] string query,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Name is required");

        if (page < 1 || pageSize < 1)
            return BadRequest("Invalid pagination parameters.");

        var result = await _userService.SearchUsersForAdminAsync(query, page, pageSize);
        return Ok(result);
    }
}
