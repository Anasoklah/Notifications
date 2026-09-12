
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
        if(request.ScheduledAt != null && request.ScheduledAt <= DateTime.UtcNow)
         throw new InvalidDataException("scheduledAt Must be in the future");
        
        if(request.Title == null && request.Body == null) 
            throw new ArgumentNullException("At least one of title or body should be not null");

        var notification = new OutboxNotification
        {
            IsBroadcast = true,
            TitleLocalized = request.Title != null ? JsonSerializer.Serialize(request.Title) : null,
            BodyLocalized = request.Body != null ? JsonSerializer.Serialize(request.Body) : null,
            Data = request.Data != null ? JsonSerializer.Serialize(request.Data) : null,
            ScheduledAt = request.ScheduledAt ?? DateTime.UtcNow,
            Status = NotificationStatus.Pending
        };

        await repo.AddNotificationAsync(notification, ct);

        return new NotificationResponseDto(
            notification.Id,
            nameof(NotificationStatus.Pending),
            1,
            notification.CreatedAt,
            notification.ScheduledAt);
    }
}
