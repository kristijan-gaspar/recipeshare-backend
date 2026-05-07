using RecipeShare.Application.DTOs.Comments;
using RecipeShare.Application.DTOs.Common;

namespace RecipeShare.Application.Interfaces.Services;

public interface ICommentService
{
    Task<CursorPagedResponse<CommentResponse>> GetPagedAsync(int recipeId, CommentQueryParameters parameters);
    Task<CommentResponse> CreateAsync(int recipeId, int userId, CommentRequest request, string actorUsername);
    Task<CommentResponse> UpdateAsync(int commentId, int userId, CommentRequest request);
    Task DeleteAsync(int commentId, int userId, bool isAdmin);
}
