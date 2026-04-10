using RecipeShare.Application.DTOs.Auth;
using RecipeShare.Domain.Entities;

namespace RecipeShare.Application.Interfaces.Services;

public interface ITokenProvider
{
    string GenerateJwtToken(User user);
    TokenResult GenerateRefreshToken();
}
