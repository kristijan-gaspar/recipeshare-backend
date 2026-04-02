using RecipeShare.Application.DTOs.Auth;
using RecipeShare.Domain.Entities;

namespace RecipeShare.Application.Interfaces.Services;

public interface IJwtProvider
{
    TokenResult GenerateToken(User user);
}
