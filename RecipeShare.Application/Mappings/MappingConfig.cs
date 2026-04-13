using Mapster;
using RecipeShare.Application.DTOs.Recipes;
using RecipeShare.Domain.Entities;

namespace RecipeShare.Application.Mappings;

public static class MappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<Recipe, RecipeSummaryResponse>.NewConfig()
            .Map(dest => dest.Author, src => new RecipeAuthorResponse
            {
                Id = src.UserId,
                Username = src.User.Username,
                ProfileImageUrl = src.User.ProfileImageUrl
            })
            .Map(dest => dest.CategoryName, src => src.Category.Name)
            .Map(dest => dest.Tags, src => src.Tags.Select(t => t.Name).ToList());

        TypeAdapterConfig<Recipe, RecipeDetailResponse>.NewConfig()
            .Map(dest => dest.Author, src => new RecipeAuthorResponse
            {
                Id = src.UserId,
                Username = src.User.Username,
                ProfileImageUrl = src.User.ProfileImageUrl
            })
            .Map(dest => dest.CategoryName, src => src.Category.Name)
            .Map(dest => dest.Tags, src => src.Tags.Select(t => t.Name).ToList())
            .Map(dest => dest.Ingredients, src => src.Ingredients)
            .Map(dest => dest.Steps, src => src.Steps);
    }
}
