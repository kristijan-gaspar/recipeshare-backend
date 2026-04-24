namespace RecipeShare.Application.DTOs.Comments;

public class CommentQueryParameters
{
    public int? Cursor { get; set; }
    public int PageSize { get; set; } = 10;
}
