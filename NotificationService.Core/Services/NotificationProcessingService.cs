using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.Logging;
using NotificationService.Core.Dtos.Delivery;
using NotificationService.Core.Dtos.DeviceToken;
using NotificationService.Core.Dtos.Notification;
using NotificationService.Core.Entities;
using NotificationService.Core.Enums;
using NotificationService.Core.Interfaces;
using NotificationService.Core.ValueObjects;

namespace NotificationService.Core.Services;

public class NotificationProcessingService(
    INotificationRepository repo,
    IFcmSender fcm,
    IValidator<RegisterTokenRequestDto> registerTokenValidator,
    IValidator<BroadcastRequestDto> broadcastValidator,
    IValidator<SendToUsersRequestDto> sendToUsersValidator,
    ILogger<NotificationProcessingService> logger)
{
    public async Task<RegisterTokenResponseDto> RegisterDeviceTokenAsync(
        RegisterTokenRequestDto request, CancellationToken ct = default)
    {
        await registerTokenValidator.ValidateAndThrowAsync(request, ct);

        var token = new DeviceToken
        {
            UserId = request.UserId,
            Token = request.Token,
            Platform = request.Platform,
            Locale = request.Locale
        };

        await repo.RegisterTokenAsync(token, ct);

        return new RegisterTokenResponseDto(
            token.UserId,
            token.Token,
            token.Platform,
            token.CreatedAt,
            token.Locale);
    }

    public async Task DeactivateTokenAsync(string token, CancellationToken ct = default) =>
        await repo.DeactivateTokenAsync(token, ct);

    public async Task DeactivateAllUserTokensAsync(string userId, CancellationToken ct = default) =>
        await repo.DeactivateAllUserTokensAsync(userId, ct);

    public async Task<NotificationResponseDto> EnqueueBroadcastAsync(
        BroadcastRequestDto request, CancellationToken ct = default)
    {
        await broadcastValidator.ValidateAndThrowAsync(request, ct);

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

    public async Task<NotificationResponseDto> EnqueueToUsersAsync(
        SendToUsersRequestDto request, CancellationToken ct = default)
    {
        await sendToUsersValidator.ValidateAndThrowAsync(request, ct);

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

    public async Task<IEnumerable<UserNotificationDto>> GetUserHistoryAsync(
        string userId, string locale = "en", CancellationToken ct = default)
    {
        var notifications = await repo.GetUserNotificationsAsync(userId, ct);

        return notifications.Select(n => new UserNotificationDto(
            NotificationId: n.Id,
            Title: n.TitleLocalized != null
                ? JsonSerializer.Deserialize<LocalizedContent>(n.TitleLocalized)?.Resolve(locale)
                : null,
            Body: !string.IsNullOrWhiteSpace(n.BodyLocalized)
                ? JsonSerializer.Deserialize<LocalizedContent>(n.BodyLocalized)?.Resolve(locale)
                : null,
            Data: !string.IsNullOrWhiteSpace(n.Data)
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(n.Data)
                : null,
            IsBroadcast: n.IsBroadcast,
            Status: n.Status.ToString(),
            CreatedAt: n.CreatedAt,
            ProcessedAt: n.ProcessedAt));
    }

    public async Task ProcessFcmBatchAsync(string workerId, CancellationToken ct = default)
    {
        await repo.ReleaseStaleLockAsync(TimeSpan.FromMinutes(5), ct);

        var batch = await repo.LockPendingBatchAsync(50, workerId, ct);
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
                    await repo.MarkAsProcessedAsync(notification.Id, ct);
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

                await repo.AddDeliveriesAsync(deliveries, ct);
                await repo.MarkAsProcessedAsync(notification.Id, ct);
            }
            catch (Exception ex)
            {
                await repo.IncrementRetryAsync(notification.Id, ex.Message, ct);
                logger.LogWarning(ex, "Failed notification {Id}", notification.Id);
            }
        }
    }

    private static bool IsInvalidTokenError(string? error) =>
        error != null && (error.Contains("UNREGISTERED") || error.Contains("INVALID_ARGUMENT"));
}