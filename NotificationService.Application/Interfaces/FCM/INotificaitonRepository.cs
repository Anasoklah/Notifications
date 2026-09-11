using NotificationService.Core.Entities;

namespace NotificationService.Application.Interfaces.FCM
{
  public interface IFCMRepository
{
    // Outbox
    Task AddNotificationAsync(OutboxNotification notification, CancellationToken ct = default);
    Task AddNotificationsAsync(IEnumerable<OutboxNotification> notifications, CancellationToken ct = default);
    Task<List<OutboxNotification>> LockPendingBatchAsync(int batchSize, string workerId, CancellationToken ct = default);
    Task MarkAsProcessedAsync(Guid notificationId, CancellationToken ct = default);
    Task MarkAsFailedAsync(Guid notificationId, string error, CancellationToken ct = default);
    Task IncrementRetryAsync(Guid notificationId, string error, CancellationToken ct = default);
    Task ReleaseStaleLockAsync(TimeSpan lockExpiry, CancellationToken ct = default);

    Task<List<OutboxNotification>> GetUserNotificationsAsync(string userId, CancellationToken ct = default);

    // Deliveries
    Task AddDeliveriesAsync(IEnumerable<NotificationDelivery> deliveries, CancellationToken ct = default);
    Task<List<NotificationDelivery>> GetDeliveriesByNotificationIdAsync(Guid notificationId, CancellationToken ct = default);
}
}