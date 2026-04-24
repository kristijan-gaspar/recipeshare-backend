using System.ComponentModel.DataAnnotations;

namespace RecipeShare.Application.DTOs.Comments;

public class CommentRequest
{
    [Required]
    [MaxLength(1000)]
    public string Content { get; set; } = null!;
}
