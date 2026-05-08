using RecipeShare.Application.DTOs.Comments;

namespace RecipeShare.Application.DTOs.Comments.Admin;

public class AdminRecipeCommentItem
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public CommentAuthorResponse Author { get; set; } = null!;
}
