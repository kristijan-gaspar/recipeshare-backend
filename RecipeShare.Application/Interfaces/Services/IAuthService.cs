using RecipeShare.Application.DTOs.Auth;

namespace RecipeShare.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshAsync(RefreshTokenRequest request);
    Task LogoutAsync(string refreshToken);
}
