using Mapster;
using RecipeShare.Application.DTOs.Comments;
using RecipeShare.Application.DTOs.Recipes;
using RecipeShare.Application.DTOs.Reports;
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

        TypeAdapterConfig<Comment, CommentResponse>.NewConfig()
            .Map(dest => dest.Author, src => new CommentAuthorResponse
            {
                Id = src.UserId,
                Username = src.User.Username,
                ProfileImageUrl = src.User.ProfileImageUrl
            });


        TypeAdapterConfig<Report, ReportResponse>.NewConfig()
            .Map(dest => dest.TargetType, src => src.TargetType.ToString())
            .Map(dest => dest.ReporterUsername, src => src.Reporter.Username)
            .Map(dest => dest.ReportedUsername, src => src.ReportedUser.Username)
            .Map(dest => dest.Reason, src => src.Reason.ToString())
            .Map(dest => dest.Status, src => src.Status.ToString());

        TypeAdapterConfig<Report, ReportDetailResponse>.NewConfig()
            .Map(dest => dest.TargetType, src => src.TargetType.ToString())
            .Map(dest => dest.ReporterUsername, src => src.Reporter.Username)
            .Map(dest => dest.ReporterId, src => src.ReporterId)
            .Map(dest => dest.ReportedUsername, src => src.ReportedUser.Username)
            .Map(dest => dest.ReportedUserId, src => src.ReportedUserId)
            .Map(dest => dest.Reason, src => src.Reason.ToString())
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.ContentAction, src => src.ContentAction.ToString())
            .Map(dest => dest.UserAction, src => src.UserAction.ToString())
            .Map(dest => dest.ResolvedByAdminUsername, src => src.ResolvedByAdmin != null
                ? src.ResolvedByAdmin.Username
                : null)
            .Ignore(dest => dest.TargetContent);
    }
}
