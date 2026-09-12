

using System.Text.Json;
using FluentValidation;
using NotificationService.Application.DTOs.Notification;
using NotificationService.Application.Interfaces.FCM;
using NotificationService.Core.Entities;
using NotificationService.Core.Enums;

namespace NotificationService.Application.UseCases.Notifiactions.FCM;

public class EnqueuToUserUseCase(
    IValidator<SendToUsersRequestDto> validator,
    IFCMRepository repo)
{
      public async Task<NotificationResponseDto> EnqueueToUsersAsync(
        SendToUsersRequestDto request, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        var notifications = request.UserIds
            .Select(userId => new OutboxNotification
            {
                TargetUserId = userId,
                IsBroadcast = false,
                TitleLocalized = request.Title != null ? JsonSerializer.Serialize(request.Title) : null,
                BodyLocalized = request.Body != null ? JsonSerializer.Serialize(request.Body) : null,
                Data = request.Data != null ? JsonSerializer.Serialize(request.Data) : null,
                ScheduledAt = request.ScheduledAt ?? DateTime.UtcNow,
                Status = NotificationStatus.Pending
            })
            .ToList();

        await repo.AddNotificationsAsync(notifications, ct);

        var first = notifications.First();
        return new NotificationResponseDto(
            first.Id,
            nameof(NotificationStatus.Pending),
            notifications.Count,
            first.CreatedAt,
            first.ScheduledAt);
    }

}
