
using System.Text.Json;
using NotificationService.Application.DTOs.Notification;
using NotificationService.Application.Interfaces.FCM;
using NotificationService.Core.Entities;
using NotificationService.Core.Enums;

namespace NotificationService.Application.UseCases.Notifiactions.FCM;

public class EnqueueBroadcastUseCase(
    IFCMRepository repo)
{
    public async Task<NotificationResponseDto> EnqueueBroadcastAsync(
        BroadcastRequestDto request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var notification = OutboxNotification.CreateBroadcast(
            request.Title != null ? JsonSerializer.Serialize(request.Title) : null,
            request.Body != null ? JsonSerializer.Serialize(request.Body) : null,
            request.Data != null ? JsonSerializer.Serialize(request.Data) : null,
            request.ScheduledAt);

        await repo.AddNotificationAsync(notification, ct);

        return new NotificationResponseDto(
            notification.Id,
            nameof(NotificationStatus.Pending),
            1,
            notification.CreatedAt,
            notification.ScheduledAt);
    }
}
