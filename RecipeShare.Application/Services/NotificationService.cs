using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;

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
}
