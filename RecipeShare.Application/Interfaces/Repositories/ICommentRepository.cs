using RecipeShare.Application.DTOs.Comments;
using RecipeShare.Domain.Entities;

namespace RecipeShare.Application.Interfaces.Repositories;

public interface ICommentRepository : IGenericRepository<Comment>
{
    Task<(IEnumerable<Comment> Items, bool HasMore)> GetCursorPagedByRecipeAsync(int recipeId, CommentQueryParameters parameters);
    Task<Comment?> GetByIdWithUserAsync(int id);
    Task<int> GetCountByRecipeAsync(int recipeId);
    Task<Dictionary<int, int>> GetCountsByRecipeIdsAsync(IEnumerable<int> recipeIds);
}
