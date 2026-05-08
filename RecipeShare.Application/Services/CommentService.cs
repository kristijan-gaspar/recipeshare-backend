using Mapster;
using Microsoft.Extensions.Logging;
using RecipeShare.Application.DTOs.Comments;
using RecipeShare.Application.DTOs.Common;
using RecipeShare.Application.Exceptions;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Domain.Entities;

namespace RecipeShare.Application.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepo;
    private readonly IRecipeRepository _recipeRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<CommentService> _logger;

    public CommentService(ICommentRepository commentRepo, IRecipeRepository recipeRepo, IUnitOfWork unitOfWork, INotificationService notificationService, ILogger<CommentService> logger)
    {
        _commentRepo = commentRepo;
        _recipeRepo = recipeRepo;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<CursorPagedResponse<CommentResponse>> GetPagedAsync(int recipeId, CommentQueryParameters parameters)
    {
        var (items, hasMore) = await _commentRepo.GetCursorPagedByRecipeAsync(recipeId, parameters);

        var mapped = items.Select(c => c.Adapt<CommentResponse>()).ToList();

        return new CursorPagedResponse<CommentResponse>
        {
            Items = mapped,
            HasMore = hasMore,
            NextCursor = hasMore ? mapped.LastOrDefault()?.Id : null
        };
    }

    public async Task<CommentResponse> CreateAsync(int recipeId, int userId, CommentRequest request, string actorUsername)
    {
        var recipe = await _recipeRepo.GetByIdAsync(recipeId);
        if (recipe == null || recipe.IsDeleted)
            throw new NotFoundException("Recipe not found");

        var comment = new Comment
        {
            Content = request.Content,
            UserId = userId,
            RecipeId = recipeId,
            CreatedAt = DateTime.UtcNow
        };

        await _commentRepo.AddAsync(comment);
        await _unitOfWork.SaveChangesAsync();

        if (recipe.UserId != userId)
        {
            try { await _notificationService.SendCommentNotificationAsync(recipe.UserId, actorUsername, recipe.Title, recipeId); }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to send comment notification for recipe {RecipeId}.", recipeId); }
        }

        var created = await _commentRepo.GetByIdWithUserAsync(comment.Id);
        return created!.Adapt<CommentResponse>();
    }

    public async Task<CommentResponse> UpdateAsync(int commentId, int userId, CommentRequest request)
    {
        var comment = await _commentRepo.GetByIdWithUserAsync(commentId);
        if (comment == null)
            throw new NotFoundException("Comment not found");

        if (comment.UserId != userId)
            throw new ForbiddenException("You can only edit your own comments");

        comment.Content = request.Content;
        comment.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return comment.Adapt<CommentResponse>();
    }

    public async Task DeleteAsync(int commentId, int userId, bool isAdmin)
    {
        var comment = await _commentRepo.GetByIdAsync(commentId);
        if (comment == null || comment.IsDeleted)
            throw new NotFoundException("Comment not found");

        if (!isAdmin && comment.UserId != userId)
            throw new ForbiddenException("You can only delete your own comments");

        comment.IsDeleted = true;
        comment.DeletedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
    }
}
