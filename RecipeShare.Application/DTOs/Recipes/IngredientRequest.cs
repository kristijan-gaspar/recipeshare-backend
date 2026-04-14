using System.ComponentModel.DataAnnotations;
using RecipeShare.Domain.Enums;

namespace RecipeShare.Application.DTOs.Recipes;

public class IngredientRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Quantity { get; set; } = string.Empty;

    public MeasurementUnit? Unit { get; set; }

    [Required]
    public int Order { get; set; }
}
