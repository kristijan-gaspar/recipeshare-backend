namespace RecipeShare.Application.DTOs.Dashboard;

public class AdminDashboardResponse
{
    public int Id { get; set; }
    public int TotalUsers { get; set; }
    public int TotalRecipes { get; set; }
    public List<int> MostActiveUsersIds { get; set; } = new();
    public List<int> MostPopularRecipesIds { get; set; } = new();
}
