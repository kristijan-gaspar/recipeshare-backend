namespace RecipeShare.Domain.Entities;

public class Rating
{
    public int Id { get; set; }
    public int Value { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
}
