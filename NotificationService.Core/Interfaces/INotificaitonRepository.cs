
using NotificationService.Core.Entities;

namespace NotificationService.Core.Interfaces
{
  public interface INotificationRepository
{
    // Device Tokens
    Task RegisterTokenAsync(DeviceToken token, CancellationToken ct = default);
    Task DeactivateTokenAsync(string token, CancellationToken ct = default);
    Task DeactivateAllUserTokensAsync(string userId, CancellationToken ct = default);
    Task<List<DeviceToken>> GetActiveTokensByUserIdAsync(string userId, CancellationToken ct = default);
    Task<List<DeviceToken>> GetAllActiveTokensAsync(CancellationToken ct = default);
    Task<bool> TokenExistsAsync(string token, CancellationToken ct = default);

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