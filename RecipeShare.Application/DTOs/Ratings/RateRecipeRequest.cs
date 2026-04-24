using System.ComponentModel.DataAnnotations;

namespace RecipeShare.Application.DTOs.Ratings;

public class RateRecipeRequest
{
    [Required]
    [Range(1, 5)]
    public int Value { get; set; }
}
