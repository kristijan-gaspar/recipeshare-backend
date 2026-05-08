namespace RecipeShare.Application.Interfaces.Services;

public interface IAdminCommentService
{
    Task SoftDeleteAsync(int commentId);
    Task RestoreAsync(int commentId);
}
