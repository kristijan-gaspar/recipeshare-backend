namespace RecipeShare.Domain.Entities;

public class DeviceToken
{
    public int Id { get; set; }
    public string Token { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
