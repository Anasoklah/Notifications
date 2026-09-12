

using NotificationService.Application.DTOs.Delivery;
using NotificationService.Application.Interfaces.FCM;

namespace NotificationService.Application.UseCases.Notifiactions.FCM;

public class GetNotificationStatusUseCase( IFCMRepository repo)
{
     public async Task<NotificationStatusResponseDto?> GetStatusAsync(
        Guid notificationId, CancellationToken ct = default)
    {
        var deliveries = await repo.GetDeliveriesByNotificationIdAsync(notificationId, ct);
        if (deliveries.Count == 0) return null;

        var first = deliveries.First();

        return new NotificationStatusResponseDto(
            notificationId,
            first.Status.ToString() ?? "UNKNOWN",
            first.OutboxNotification?.RetryCount ?? 0,
            first.OutboxNotification?.LastError,
            first.OutboxNotification?.CreatedAt ?? default,
            first.OutboxNotification?.ProcessedAt,
            [.. deliveries.Select(d => new DeliveryDetailDto(
                d.Id,
                d.Token,
                d.Status.ToString(),
                d.AttemptNumber,
                d.SentAt,
                d.FcmMessageId,
                d.Error))]);
    }
    
}
