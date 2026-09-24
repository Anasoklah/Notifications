using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces.Smtp;
using NotificationService.Core.Entities;
using NotificationService.Core.Enums;
using NotificationService.Infrastructure.Data;

namespace NotificationService.Infrastructure.Repositories
{
    public class SmptRepository(AppDbContext db) : ISmtpRepository
    {
        public async Task AddOutboxEmailAsync(OutboxEmail email, CancellationToken ct = default)
        {
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
                    item.MarkProcessing(workerId, now);

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
            var email = await db.OutboxEmails
                .FirstOrDefaultAsync(e => e.Id == outboxEmailId, ct);

            email?.MarkProcessed(DateTime.UtcNow);
            await db.SaveChangesAsync(ct);
        }

        public async Task IncrementEmailRetryAsync(Guid outboxEmailId, string error, CancellationToken ct = default)
        {
            var email = await db.OutboxEmails.FirstOrDefaultAsync(e => e.Id == outboxEmailId, ct);
            if (email is null) return;

            email.RecordFailure(error);

            await db.SaveChangesAsync(ct);
        }

        public async Task ReleaseStaleEmailLockAsync(TimeSpan lockExpiry, CancellationToken ct = default)
        {
            var threshold = DateTime.UtcNow - lockExpiry;

            var emails = await db.OutboxEmails
                .Where(e =>
                    e.Status == NotificationStatus.Processing &&
                    e.LockedAt != null &&
                    e.LockedAt < threshold)
                .ToListAsync(ct);

            foreach (var email in emails)
                email.ReleaseStaleLock();

            await db.SaveChangesAsync(ct);
        }

        public async Task AddEmailDeliveriesAsync(IEnumerable<EmailDelivery> deliveries, CancellationToken ct = default)
        {
            var list = deliveries.ToList();

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
