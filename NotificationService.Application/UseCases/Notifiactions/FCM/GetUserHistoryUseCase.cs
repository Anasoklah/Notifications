

using System.Text.Json;
using NotificationService.Application.DTOs.Notification;
using NotificationService.Application.Interfaces.FCM;
using NotificationService.Core.ValueObjects;

namespace NotificationService.Application.UseCases.Notifiactions.FCM;

public class GetUserHistoryUseCase(IFCMRepository repo)
{
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
}
