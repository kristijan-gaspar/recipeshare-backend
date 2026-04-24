namespace RecipeShare.Application.DTOs.Comments;

public class CommentAuthorResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string? ProfileImageUrl { get; set; }
}
