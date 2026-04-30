using Mapster;
using RecipeShare.Application.DTOs.Common;
using RecipeShare.Application.DTOs.Recipes;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Domain.Entities;

namespace RecipeShare.Application.Services;

public class RecipeResponseMapper : IRecipeResponseMapper
{
    private readonly IRecipeSocialStatsService _socialStats;

    public RecipeResponseMapper(IRecipeSocialStatsService socialStats)
    {
        _socialStats = socialStats;
    }

    public async Task<CursorPagedResponse<RecipeSummaryResponse>> ToCursorPagedAsync(
        IEnumerable<Recipe> recipes,
        bool hasMore,
        int userId)
    {
        var list = recipes.ToList();

        var mapped = list
            .Select(r => r.Adapt<RecipeSummaryResponse>())
            .ToList();

        await _socialStats.ApplyStatsAsync(mapped, userId);

        return new CursorPagedResponse<RecipeSummaryResponse>
        {
            Items = mapped,
            NextCursor = hasMore ? list.LastOrDefault()?.Id : null,
            HasMore = hasMore
        };
    }
}
