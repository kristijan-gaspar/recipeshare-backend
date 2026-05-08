using RecipeShare.Application.Exceptions;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;

namespace RecipeShare.Application.Services;

public class AdminCommentService : IAdminCommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AdminCommentService(ICommentRepository commentRepository, IUnitOfWork unitOfWork)
    {
        _commentRepository = commentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task SoftDeleteAsync(int commentId)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null || comment.IsDeleted)
            throw new NotFoundException("Comment not found.");

        comment.IsDeleted = true;
        comment.DeletedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RestoreAsync(int commentId)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null)
            throw new NotFoundException("Comment not found.");

        if (!comment.IsDeleted)
            throw new BadRequestException("Comment is not deleted.");

        comment.IsDeleted = false;
        comment.DeletedAt = null;

        await _unitOfWork.SaveChangesAsync();
    }
}
