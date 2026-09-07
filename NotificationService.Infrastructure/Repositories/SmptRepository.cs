using Microsoft.EntityFrameworkCore;
using NotificationService.Core.Entities;
using NotificationService.Core.Enums;
using NotificationService.Core.Interfaces;
using NotificationService.Infrastructure.Data;

namespace NotificationService.Infrastructure.Repositories
{
    public class SmptRepository(AppDbContext db) : ISmtpRepository
    {
        public async Task AddOutboxEmailAsync(OutboxEmail email, CancellationToken ct = default)
        {
            email.Id = Guid.NewGuid();
            email.CreatedAt = DateTime.UtcNow;
            email.Status = NotificationStatus.Pending;

            await db.OutboxEmails.AddAsync(email, ct);
            await db.SaveChangesAsync(ct);
        }

        public async Task<List<OutboxEmail>> LockPendingEmailBatchAsync(
            int batchSize, string workerId, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var lockExpiry = now.AddMinutes(-5);

            await using var transaction = await db.Database.BeginTransactionAsync(ct);

            try
            {
                var batch = await db.OutboxEmails
                    .Where(e =>
                        e.Status == NotificationStatus.Pending &&
                        (e.ScheduledAt == null || e.ScheduledAt <= now) &&
                        e.RetryCount < e.MaxRetries &&
                        (e.LockedAt == null || e.LockedAt < lockExpiry))
                    .OrderBy(e => e.CreatedAt)
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

        public async Task MarkEmailAsProcessedAsync(Guid outboxEmailId, CancellationToken ct = default)
        {
            await db.OutboxEmails
                .Where(e => e.Id == outboxEmailId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(e => e.Status, NotificationStatus.Sent)
                    .SetProperty(e => e.ProcessedAt, DateTime.UtcNow)
                    .SetProperty(e => e.LockedAt, (DateTime?)null)
                    .SetProperty(e => e.LockedBy, (string?)null), ct);
        }

        public async Task IncrementEmailRetryAsync(Guid outboxEmailId, string error, CancellationToken ct = default)
        {
            var email = await db.OutboxEmails.FirstOrDefaultAsync(e => e.Id == outboxEmailId, ct);
            if (email is null) return;

            email.RetryCount++;
            email.LastError = error;
            email.LockedAt = null;
            email.LockedBy = null;

            email.Status = email.RetryCount >= email.MaxRetries
                ? NotificationStatus.Failed
                : NotificationStatus.Pending;

            await db.SaveChangesAsync(ct);
        }

        public async Task ReleaseStaleEmailLockAsync(TimeSpan lockExpiry, CancellationToken ct = default)
        {
            var threshold = DateTime.UtcNow - lockExpiry;

            await db.OutboxEmails
                .Where(e =>
                    e.Status == NotificationStatus.Processing &&
                    e.LockedAt != null &&
                    e.LockedAt < threshold)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(e => e.Status, NotificationStatus.Pending)
                    .SetProperty(e => e.LockedAt, (DateTime?)null)
                    .SetProperty(e => e.LockedBy, (string?)null), ct);
        }

        public async Task AddEmailDeliveriesAsync(IEnumerable<EmailDelivery> deliveries, CancellationToken ct = default)
        {
            var list = deliveries.ToList();

            foreach (var d in list)
                d.Id = Guid.NewGuid();

            await db.EmailDeliveries.AddRangeAsync(list, ct);
            await db.SaveChangesAsync(ct);
        }

        public async Task<List<EmailDelivery>> GetEmailDeliveriesByOutboxIdAsync(Guid outboxEmailId, CancellationToken ct = default)
        {
            return await db.EmailDeliveries
                .Where(d => d.OutboxEmailId == outboxEmailId)
                .AsNoTracking()
                .ToListAsync(ct);
        }
    }
}
