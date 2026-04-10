using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeShare.API.Extensions;
using RecipeShare.Application.DTOs.Users;
using RecipeShare.Application.Interfaces.Services;

namespace RecipeShare.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
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

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
    {
        var currentUserId = User.GetUserId();
        await _userService.UpdateProfileAsync(currentUserId, request);
        return NoContent();
    }

    [HttpPut("me/password")]
    public async Task<IActionResult> ChangeMyPassword([FromBody] ChangePasswordRequest request)
    {
        var currentUserId = User.GetUserId();
        await _userService.ChangePasswordAsync(currentUserId, request);
        return NoContent();
    }

    [HttpPut("me/email")]
    public async Task<IActionResult> ChangeMyEmail([FromBody] ChangeEmailRequest request)
    {
        var currentUserId = User.GetUserId();
        await _userService.ChangeEmailAsync(currentUserId, request);
        return NoContent();
    }

    [HttpPut("me/image")]
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

    [HttpDelete("me/image")]
    public async Task<IActionResult> DeleteMyProfileImage()
    {
        var currentUserId = User.GetUserId();
        await _userService.DeleteProfileImageAsync(currentUserId);
        return NoContent();
    }
}
