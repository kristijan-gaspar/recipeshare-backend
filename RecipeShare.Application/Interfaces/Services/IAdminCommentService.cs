namespace RecipeShare.Application.Interfaces.Services;

public interface IAdminCommentService
{
    Task SoftDeleteAsync(int commentId);
}
