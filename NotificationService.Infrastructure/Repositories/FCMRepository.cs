
using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces.FCM;
using NotificationService.Core.Entities;
using NotificationService.Core.Enums;
using NotificationService.Infrastructure.Data;

namespace NotificationService.Infrastructure.Repositories
{
    public class FCMRepository(AppDbContext db) : IFCMRepository
{
  

   
    // -------------------------------------------------------------------------
    // Outbox
    // -------------------------------------------------------------------------


        public async Task<List<OutboxNotification>> GetUserNotificationsAsync(string userId, CancellationToken ct = default)
        {
            return await db.OutboxNotifications
                .Where(n => n.TargetUserId == userId)
                .AsNoTracking()
                .ToListAsync(ct);
        }

    public async Task AddNotificationAsync(OutboxNotification notification, CancellationToken ct = default)
    {
        notification.Id = Guid.NewGuid();
        notification.CreatedAt = DateTime.UtcNow;
        notification.Status = NotificationStatus.Pending;

        await db.OutboxNotifications.AddAsync(notification, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddNotificationsAsync(
        IEnumerable<OutboxNotification> notifications, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var list = notifications.ToList();

        foreach (var n in list)
        {
            n.Id = Guid.NewGuid();
            n.CreatedAt = now;
            n.Status = NotificationStatus.Pending;
        }

        await db.OutboxNotifications.AddRangeAsync(list, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<OutboxNotification>> LockPendingBatchAsync(
        int batchSize, string workerId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var lockExpiry = now.AddMinutes(-5);

        // Use an explicit transaction to safely select + lock
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        try
        {
            var batch = await db.OutboxNotifications
                .Where(n =>
                    n.Status == NotificationStatus.Pending &&
                    (n.ScheduledAt == null || n.ScheduledAt <= now) &&
                    n.RetryCount < n.MaxRetries &&
                    (n.LockedAt == null || n.LockedAt < lockExpiry))
                .OrderBy(n => n.CreatedAt)
                .Take(batchSize)
                .ToListAsync(ct);

            if (batch.Count == 0)
            {
                await transaction.RollbackAsync(ct);
                return batch;
            }

            foreach (var item in batch)
            {
                item.Status = NotificationStatus.Processing;
                item.LockedAt = now;
                item.LockedBy = workerId;
            }

            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return batch;
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task MarkAsProcessedAsync(Guid notificationId, CancellationToken ct = default)
    {
        await db.OutboxNotifications
            .Where(n => n.Id == notificationId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.Status, NotificationStatus.Sent)
                .SetProperty(n => n.ProcessedAt, DateTime.UtcNow)
                .SetProperty(n => n.LockedAt, (DateTime?)null)
                .SetProperty(n => n.LockedBy, (string?)null), ct);
    }

    public async Task MarkAsFailedAsync(Guid notificationId, string error, CancellationToken ct = default)
    {
        await db.OutboxNotifications
            .Where(n => n.Id == notificationId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.Status, NotificationStatus.Failed)
                .SetProperty(n => n.ProcessedAt, DateTime.UtcNow)
                .SetProperty(n => n.LastError, error)
                .SetProperty(n => n.LockedAt, (DateTime?)null)
                .SetProperty(n => n.LockedBy, (string?)null), ct);
    }

    public async Task IncrementRetryAsync(Guid notificationId, string error, CancellationToken ct = default)
    {
        // Load to check MaxRetries before deciding final status
        var notification = await db.OutboxNotifications
            .FirstOrDefaultAsync(n => n.Id == notificationId, ct);

        if (notification is null) return;

        notification.RetryCount++;
        notification.LastError = error;
        notification.LockedAt = null;
        notification.LockedBy = null;

        notification.Status = notification.RetryCount >= notification.MaxRetries
            ? NotificationStatus.Failed
            : NotificationStatus.Pending;

        await db.SaveChangesAsync(ct);
    }

    public async Task ReleaseStaleLockAsync(TimeSpan lockExpiry, CancellationToken ct = default)
    {
        var threshold = DateTime.UtcNow - lockExpiry;

        await db.OutboxNotifications
            .Where(n =>
                n.Status == NotificationStatus.Processing &&
                n.LockedAt != null &&
                n.LockedAt < threshold)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.Status, NotificationStatus.Pending)
                .SetProperty(n => n.LockedAt, (DateTime?)null)
                .SetProperty(n => n.LockedBy, (string?)null), ct);
    }




    // -------------------------------------------------------------------------
    // Deliveries
    // -------------------------------------------------------------------------

    public async Task AddDeliveriesAsync(
        IEnumerable<NotificationDelivery> deliveries, CancellationToken ct = default)
    {
        var list = deliveries.ToList();

        foreach (var d in list)
            d.Id = Guid.NewGuid();

        await db.NotificationDeliveries.AddRangeAsync(list, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<NotificationDelivery>> GetDeliveriesByNotificationIdAsync(
        Guid notificationId, CancellationToken ct = default)
    {
        return await db.NotificationDeliveries
            .Where(d => d.OutboxNotificationId == notificationId)
            .AsNoTracking()
            .ToListAsync(ct);
    }
}
}