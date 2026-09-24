

using NotificationService.Core.Enums;

namespace NotificationService.Core.Entities
{
 public class NotificationDelivery
{
    public Guid Id { get; private set; }
    public Guid OutboxNotificationId { get; private set; }
    public Guid DeviceTokenId { get; private set; }
    public string Token { get; private set; } = default!;
    public NotificationStatus Status { get; private set; }
    public int AttemptNumber { get; private set; }
    public DateTime? SentAt { get; private set; }
    public string? Error { get; private set; }
    public string? FcmMessageId { get; private set; }
    
    public OutboxNotification OutboxNotification { get; private set; } = default!;

    private NotificationDelivery() { }

    public static NotificationDelivery Create(
        Guid outboxNotificationId,
        Guid deviceTokenId,
        string token,
        NotificationStatus status,
        int attemptNumber,
        DateTime? sentAt = null,
        string? fcmMessageId = null,
        string? error = null)
    {
        if (outboxNotificationId == Guid.Empty)
            throw new ArgumentException("Outbox notification id is required.", nameof(outboxNotificationId));

        if (deviceTokenId == Guid.Empty)
            throw new ArgumentException("Device token id is required.", nameof(deviceTokenId));

        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token is required.", nameof(token));

        if (attemptNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(attemptNumber));

        if (!Enum.IsDefined(status))
            throw new ArgumentException("Delivery status is invalid.", nameof(status));

        return new NotificationDelivery
        {
            Id = Guid.NewGuid(),
            OutboxNotificationId = outboxNotificationId,
            DeviceTokenId = deviceTokenId,
            Token = token,
            Status = status,
            AttemptNumber = attemptNumber,
            SentAt = sentAt,
            FcmMessageId = fcmMessageId,
            Error = error
        };
    }
}
}