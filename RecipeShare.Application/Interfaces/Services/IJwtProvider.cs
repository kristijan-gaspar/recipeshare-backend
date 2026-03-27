using RecipeShare.Domain.Entities;

namespace RecipeShare.Application.Interfaces.Services;

public interface IJwtProvider
{
    string GenerateToken(User user);
}
