using RecipeShare.Domain.Enums;

namespace RecipeShare.Domain.Entities;

public class Ingredient
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public MeasurementUnit? Unit { get; set; }
    public int Order { get; set; }

    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
}
