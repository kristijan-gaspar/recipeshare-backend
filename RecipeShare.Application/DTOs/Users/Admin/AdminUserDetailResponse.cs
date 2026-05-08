using RecipeShare.Domain.Enums;

namespace RecipeShare.Application.DTOs.Users.Admin;

public class AdminUserDetailResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public UserRole Role { get; set; }

    public int RecipeCount { get; set; }
    public int CommentCount { get; set; }
    public int LikeCount { get; set; }
    public int RatingCount { get; set; }
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }

    public IReadOnlyList<AdminUserRecipeItem> RecentRecipes { get; set; } = [];
    public IReadOnlyList<AdminUserCommentItem> RecentComments { get; set; } = [];
}
