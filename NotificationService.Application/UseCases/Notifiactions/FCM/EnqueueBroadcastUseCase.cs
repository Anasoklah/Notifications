using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentValidation;
using NotificationService.Application.DTOs.Notification;
using NotificationService.Application.Interfaces.FCM;
using NotificationService.Core.Entities;
using NotificationService.Core.Enums;

namespace NotificationService.Application.UseCases.Notifiactions.FCM;

public class EnqueueBroadcastUseCase(
    IValidator<BroadcastRequestDto> validator,
    IFCMRepository repo)
{
      public async Task<NotificationResponseDto> EnqueueBroadcastAsync(
        BroadcastRequestDto request, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(request, ct);

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
