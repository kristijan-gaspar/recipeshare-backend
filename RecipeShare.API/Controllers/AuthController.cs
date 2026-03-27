using Microsoft.AspNetCore.Mvc;
using RecipeShare.Application.DTOs.Auth;
using RecipeShare.Application.Interfaces.Services;

namespace RecipeShare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
        => Ok(await _authService.RegisterAsync(request));

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
        => Ok(await _authService.LoginAsync(request));
}
