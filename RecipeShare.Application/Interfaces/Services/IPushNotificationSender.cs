namespace RecipeShare.Application.Interfaces.Services;

public interface IPushNotificationSender
{
    Task SendAsync(IEnumerable<string> deviceTokens, string title, string body, Dictionary<string, string>? data = null);
}
