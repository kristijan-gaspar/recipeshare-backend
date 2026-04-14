namespace RecipeShare.Application.DTOs.Recipes;

public class RecipeAuthorResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
}
