using RecipeShare.Domain.Enums;

namespace RecipeShare.Application.DTOs.Recipes;

public class IngredientResponse
{
    public string Name { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public MeasurementUnit? Unit { get; set; }
    public int Order { get; set; }
}
