using NotificationService.Core.Entities;

namespace NotificationService.Application.Interfaces.Smtp
{
    public interface ISmtpRepository
    {
        Task AddOutboxEmailAsync(OutboxEmail email, CancellationToken ct = default);
        Task<List<OutboxEmail>> LockPendingEmailBatchAsync(int batchSize, string workerId, CancellationToken ct = default);
        Task MarkEmailAsProcessedAsync(Guid outboxEmailId, CancellationToken ct = default);
        Task IncrementEmailRetryAsync(Guid outboxEmailId, string error, CancellationToken ct = default);
        Task ReleaseStaleEmailLockAsync(TimeSpan lockExpiry, CancellationToken ct = default);
        Task AddEmailDeliveriesAsync(IEnumerable<EmailDelivery> deliveries, CancellationToken ct = default);

    }
}