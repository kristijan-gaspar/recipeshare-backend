using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeShare.API.Extensions;
using RecipeShare.Application.DTOs.Notifications;
using RecipeShare.Application.Interfaces.Services;

namespace RecipeShare.API.Controllers;

[ApiController]
[Route("api/device-tokens")]
[Authorize]
public class DeviceTokensController : ControllerBase
{
    private readonly IDeviceTokenService _deviceTokenService;

    public DeviceTokensController(IDeviceTokenService deviceTokenService)
    {
        _deviceTokenService = deviceTokenService;
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterDeviceTokenRequest request)
    {
        var userId = User.GetUserId();
        await _deviceTokenService.RegisterAsync(userId, request);
        return NoContent();
    }

    [HttpDelete("{token}")]
    public async Task<IActionResult> Unregister(string token)
    {
        await _deviceTokenService.UnregisterAsync(token);
        return NoContent();
    }
}
