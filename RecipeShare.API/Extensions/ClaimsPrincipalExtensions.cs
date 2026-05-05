using System.Security.Claims;
using RecipeShare.Application.Exceptions;
using RecipeShare.Domain.Enums;

namespace RecipeShare.API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) :
                throw new UnauthorizedException("User is not authenticated");
        }

        public static UserRole GetUserRole(this ClaimsPrincipal user)
        {
            var roleClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            return roleClaim != null ? Enum.Parse<UserRole>(roleClaim.Value) :
                throw new UnauthorizedException("User is not authenticated");
        }

        public static string GetUsername(this ClaimsPrincipal user)
        {
            var claim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            return claim?.Value ?? throw new UnauthorizedException("User is not authenticated");
        }

    }
}
