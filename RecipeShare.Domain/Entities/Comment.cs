namespace RecipeShare.Domain.Entities;

public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; } = null!;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
}
