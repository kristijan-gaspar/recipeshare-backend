using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeShare.API.Extensions;
using RecipeShare.Application.Common;
using RecipeShare.Application.DTOs.Users;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Domain.Enums;

namespace RecipeShare.API.Controllers;

[ApiController]
[Route("api/user")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IFollowService _followService;

    public UserController(IUserService userService, IFollowService followService)
    {
        _userService = userService;
        _followService = followService;
    }

    [HttpGet]
    public async Task<ActionResult<UserProfileResponse>> GetMyProfile()
    {
        var currentUserId = User.GetUserId();
        var profile = await _userService.GetProfileAsync(currentUserId, currentUserId);
        return Ok(profile);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserProfileResponse>> GetById(int id)
    {
        int? currentUserId = User.Identity?.IsAuthenticated == true
            ? User.GetUserId()
            : null;

        var profile = await _userService.GetProfileAsync(id, currentUserId);
        return Ok(profile);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
    {
        var currentUserId = User.GetUserId();
        await _userService.UpdateProfileAsync(currentUserId, request);
        return NoContent();
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangeMyPassword([FromBody] ChangePasswordRequest request)
    {
        var currentUserId = User.GetUserId();
        await _userService.ChangePasswordAsync(currentUserId, request);
        return NoContent();
    }

    [HttpPut("email")]
    public async Task<IActionResult> ChangeMyEmail([FromBody] ChangeEmailRequest request)
    {
        var currentUserId = User.GetUserId();
        await _userService.ChangeEmailAsync(currentUserId, request);
        return NoContent();
    }

    [HttpPut("image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateMyProfileImage(IFormFile image)
    {
        if (image is null || image.Length == 0)
            return BadRequest("Image file is required.");

        var currentUserId = User.GetUserId();

        await using var imageStream = image.OpenReadStream();
        await _userService.UpdateProfileImageAsync(currentUserId, imageStream, image.FileName);

        return NoContent();
    }

    [HttpDelete("image")]
    public async Task<IActionResult> DeleteMyProfileImage()
    {
        var currentUserId = User.GetUserId();
        await _userService.DeleteProfileImageAsync(currentUserId);
        return NoContent();
    }

    [HttpPost("{id:int}/follow")]
    public async Task<ActionResult<bool>> ToggleFollow(int id)
    {
        var currentUserId = User.GetUserId();
        var username = User.GetUsername();
        var result = await _followService.ToggleFollowAsync(id, currentUserId, username);
        return Ok(result);
    } 


    [HttpDelete]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request)
    {
        var currentUserId = User.GetUserId();
        var isAdmin = User.GetUserRole() == UserRole.Admin;
        await _userService.DeleteAccountAsync(currentUserId, request, isAdmin);
        return NoContent();
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedResponse<UserSearchResponse>>> SearchUsers(
    [FromQuery] string query,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Name is required");

        var result = await _userService.SearchUsersAsync(query, page, pageSize);
        return Ok(result);
    }

}
