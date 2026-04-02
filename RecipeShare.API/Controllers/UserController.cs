using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeShare.Application.DTOs.Users;
using RecipeShare.Application.Interfaces.Services;

namespace RecipeShare.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserProfileResponse>> GetMyProfile()
    {
        var currentUserId = GetCurrentUserId();
        var profile = await _userService.GetProfileAsync(currentUserId, currentUserId);
        return Ok(profile);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserProfileResponse>> GetById(int id)
    {
        int? currentUserId = null;

        if (User.Identity?.IsAuthenticated == true)
        {
            currentUserId = GetCurrentUserId();
        }

        var profile = await _userService.GetProfileAsync(id, currentUserId);
        return Ok(profile);
    }

    [HttpPut("me")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateMyProfile(
        [FromForm] UpdateProfileRequest request,
        IFormFile? image)
    {
        if (image is not null)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest("Dozvoljeni formati slike su: .jpg, .jpeg, .png, .webp");
            }

            const long maxFileSize = 5 * 1024 * 1024; // 5 MB
            if (image.Length > maxFileSize)
            {
                return BadRequest("Maksimalna veličina slike je 5 MB.");
            }
        }

        var currentUserId = GetCurrentUserId();

        Stream? imageStream = null;
        string? imageName = null;

        if (image is not null)
        {
            imageStream = image.OpenReadStream();
            imageName = image.FileName;
        }

        try
        {
            await _userService.UpdateProfileAsync(currentUserId, request, imageStream, imageName);
        }
        finally
        {
            if (imageStream is not null)
                await imageStream.DisposeAsync();
        }

        return NoContent();
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID nije pronađen u tokenu.");
        }

        return userId;
    }
}