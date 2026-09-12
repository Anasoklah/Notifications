
using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces.FCM;
using NotificationService.Application.Interfaces.Processors;
using NotificationService.Application.Interfaces.Tokens;
using NotificationService.Core.Entities;
using NotificationService.Core.Enums;

namespace NotificationService.Infrastructure.Processors;


public class NotificationProcessor(
    ITokensRepository repo,
    IFCMRepository fCMRepository,
    IFcmSender fcm,
    ILogger<NotificationProcessor> logger) : INotificationProcessor
{
   
    public async Task ProcessFcmBatchAsync(string workerId, CancellationToken ct = default)
    {
        await fCMRepository.ReleaseStaleLockAsync(TimeSpan.FromMinutes(5), ct);

        var batch = await fCMRepository.LockPendingBatchAsync(50, workerId, ct);
        if (batch.Count == 0) return;

        foreach (var notification in batch)
        {
            try
            {
                var tokens = notification.IsBroadcast
                    ? await repo.GetAllActiveTokensAsync(ct)
                    : await repo.GetActiveTokensByUserIdAsync(notification.TargetUserId!, ct);

                if (tokens.Count == 0)
                {
                    await fCMRepository.MarkAsProcessedAsync(notification.Id, ct);
                    continue;
                }

                var deliveries = new List<NotificationDelivery>();

                foreach (var token in tokens)
                {
                    var title = notification.ResolcveTitle(token.Locale);
                    var body = notification.ResolcveBody(token.Locale);
                    var (success, messageId, error) = await fcm.SendAsync(
                        token.Token, title, body, notification.Data, ct);

                    deliveries.Add(new NotificationDelivery
                    {
                        OutboxNotificationId = notification.Id,
                        DeviceTokenId = token.Id,
                        Token = token.Token,
                        Status = success ? NotificationStatus.Sent : NotificationStatus.Failed,
                        SentAt = success ? DateTime.UtcNow : null,
                        AttemptNumber = notification.RetryCount + 1,
                        FcmMessageId = messageId,
                        Error = error
                    });

                    if (!success && IsInvalidTokenError(error))
                        await repo.DeactivateTokenAsync(token.Token, ct);
                }

                await fCMRepository.AddDeliveriesAsync(deliveries, ct);
                await fCMRepository.MarkAsProcessedAsync(notification.Id, ct);
            }
            catch (Exception ex)
            {
                await fCMRepository.IncrementRetryAsync(notification.Id, ex.Message, ct);
                logger.LogWarning(ex, "Failed notification {Id}", notification.Id);
            }
        }
    }

    private static bool IsInvalidTokenError(string? error) =>
        error != null && (error.Contains("UNREGISTERED") || error.Contains("INVALID_ARGUMENT"));
}