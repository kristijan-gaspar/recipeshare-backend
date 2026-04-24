namespace RecipeShare.Application.DTOs.Comments;

public class CommentResponse
{
    public int Id { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public CommentAuthorResponse Author { get; set; } = null!;
}
