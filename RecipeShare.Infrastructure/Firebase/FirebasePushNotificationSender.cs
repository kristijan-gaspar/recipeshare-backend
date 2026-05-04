using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;

namespace RecipeShare.Infrastructure.Firebase;

public class FirebasePushNotificationSender : IPushNotificationSender
{
    private readonly IDeviceTokenRepository _deviceTokenRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FirebasePushNotificationSender> _logger;

    public FirebasePushNotificationSender(
        IDeviceTokenRepository deviceTokenRepo,
        IUnitOfWork unitOfWork,
        ILogger<FirebasePushNotificationSender> logger)
    {
        _deviceTokenRepo = deviceTokenRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task SendAsync(IEnumerable<string> deviceTokens, string title, string body, Dictionary<string, string>? data = null)
    {
        var tokens = deviceTokens.ToList();
        if (tokens.Count == 0) return;

        var message = new MulticastMessage
        {
            Tokens = tokens,
            Notification = new Notification { Title = title, Body = body },
            Data = data
        };

        var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);

        if (response.FailureCount == 0) return;

        var staleTokens = new List<string>();
        for (var i = 0; i < response.Responses.Count; i++)
        {
            var result = response.Responses[i];
            if (!result.IsSuccess &&
                result.Exception.MessagingErrorCode is MessagingErrorCode.Unregistered or MessagingErrorCode.InvalidArgument)
            {
                staleTokens.Add(tokens[i]);
            }
        }

        foreach (var token in staleTokens)
        {
            await _deviceTokenRepo.DeleteByTokenAsync(token);
            _logger.LogInformation("Removed stale FCM token {Token}.", token);
        }

        if (staleTokens.Count > 0)
            await _unitOfWork.SaveChangesAsync();
    }
}
