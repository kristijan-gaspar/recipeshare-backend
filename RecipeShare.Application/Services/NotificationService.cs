using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Domain.Enums;

namespace RecipeShare.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IDeviceTokenRepository _deviceTokenRepo;
    private readonly IPushNotificationSender _sender;

    public NotificationService(IDeviceTokenRepository deviceTokenRepo, IPushNotificationSender sender)
    {
        _deviceTokenRepo = deviceTokenRepo;
        _sender = sender;
    }

    public async Task SendLikeNotificationAsync(int recipientUserId, string likerUsername, string recipeTitle, int recipeId)
    {
        var tokens = await _deviceTokenRepo.GetTokensByUserIdAsync(recipientUserId);
        if (tokens.Count == 0) return;

        await _sender.SendAsync(tokens,
            "New like",
            $"{likerUsername} liked your recipe \"{recipeTitle}\"",
            new Dictionary<string, string> { ["type"] = "like", ["recipeId"] = recipeId.ToString() });
    }

    public async Task SendCommentNotificationAsync(int recipientUserId, string commenterUsername, string recipeTitle, int recipeId)
    {
        var tokens = await _deviceTokenRepo.GetTokensByUserIdAsync(recipientUserId);
        if (tokens.Count == 0) return;

        await _sender.SendAsync(tokens,
            "New comment",
            $"{commenterUsername} commented on \"{recipeTitle}\"",
            new Dictionary<string, string> { ["type"] = "comment", ["recipeId"] = recipeId.ToString() });
    }

    public async Task SendFollowNotificationAsync(int recipientUserId, string followerUsername, int followerId)
    {
        var tokens = await _deviceTokenRepo.GetTokensByUserIdAsync(recipientUserId);
        if (tokens.Count == 0) return;

        await _sender.SendAsync(tokens,
            "New follower",
            $"{followerUsername} started following you",
            new Dictionary<string, string> { ["type"] = "follow", ["followerId"] = followerId.ToString() });
    }

    public async Task SendWarningNotificationAsync(int recipientUserId, ReportReason reason)
    {
        var tokens = await _deviceTokenRepo.GetTokensByUserIdAsync(recipientUserId);
        if (tokens.Count == 0) return;

        var message = reason switch
        {
            ReportReason.Spam =>
                "Your content was reported and reviewed as spam. Please avoid posting repetitive or promotional content.",
            ReportReason.OffensiveContent =>
                "Your content was reported and reviewed as offensive. Please keep your interactions respectful.",
            ReportReason.InappropriateContent =>
                "Your content was reported and reviewed as inappropriate. Please make sure your content follows the community rules.",
            _ =>
                "Your content was reported and reviewed by an administrator. Please make sure your future activity follows the community rules."
        };

        await _sender.SendAsync(
            tokens,
            "Warning from admin",
            message,
            new Dictionary<string, string>
            {
                ["type"] = "warning",
                ["reason"] = reason.ToString()
            });

    }
}

