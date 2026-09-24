
using NotificationService.Core.Enums;

namespace NotificationService.Core.Entities;

   public class EmailDelivery
{
    public Guid Id { get; private set; }
    public Guid OutboxEmailId { get; private set; }
    public NotificationStatus Status { get; private set; }
    public DateTime? SentAt { get; private set; }
    public string? Error { get; private set; }
    public int AttemptNumber { get; private set; }
    public OutboxEmail OutboxEmail { get; private set; } = default!;

    private EmailDelivery() { }

    public static EmailDelivery Create(
        Guid outboxEmailId,
        NotificationStatus status,
        int attemptNumber,
        DateTime? sentAt = null,
        string? error = null)
    {
        if (outboxEmailId == Guid.Empty)
            throw new ArgumentException("Outbox email id is required.", nameof(outboxEmailId));

        if (attemptNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(attemptNumber));

        if (!Enum.IsDefined(status))
            throw new ArgumentException("Delivery status is invalid.", nameof(status));

        return new EmailDelivery
        {
            Id = Guid.NewGuid(),
            OutboxEmailId = outboxEmailId,
            Status = status,
            AttemptNumber = attemptNumber,
            SentAt = sentAt,
            Error = error
        };
    }
}
