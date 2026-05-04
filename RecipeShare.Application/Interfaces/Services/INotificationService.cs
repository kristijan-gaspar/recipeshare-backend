namespace RecipeShare.Application.Interfaces.Services;

public interface INotificationService
{
    Task SendLikeNotificationAsync(int recipientUserId, string likerUsername, string recipeTitle, int recipeId);
    Task SendCommentNotificationAsync(int recipientUserId, string commenterUsername, string recipeTitle, int recipeId);
    Task SendFollowNotificationAsync(int recipientUserId, string followerUsername, int followerId);
}
