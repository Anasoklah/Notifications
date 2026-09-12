

using System.Text.Json;
using NotificationService.Application.DTOs.Notification;
using NotificationService.Application.Interfaces.FCM;
using NotificationService.Core.Entities;
using NotificationService.Core.Enums;

namespace NotificationService.Application.UseCases.Notifiactions.FCM;

public class EnqueuToUserUseCase(IFCMRepository repo)
{
      public async Task<NotificationResponseDto> EnqueueToUsersAsync(
        SendToUsersRequestDto request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.UserIds is null || request.UserIds.Count == 0)
            throw new ArgumentException("At least one UserId is required.", nameof(request));

        if (request.UserIds.Count > 1000)
            throw new ArgumentException("Cannot target more than 1000 users per request.", nameof(request));

        if (request.UserIds.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("UserIds must not contain empty values.", nameof(request));

        if (request.Title is null && request.Body is null)
            throw new ArgumentException("At least Title or Body is required.", nameof(request));

        if (request.ScheduledAt.HasValue && request.ScheduledAt.Value <= DateTime.UtcNow)
            throw new ArgumentException("ScheduledAt must be in the future.", nameof(request));

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
