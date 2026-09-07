

using NotificationService.Core.Enums;

namespace NotificationService.Core.Entities
{
 public class NotificationDelivery
{
    public Guid Id { get; set; }
    public Guid OutboxNotificationId { get; set; }
    public Guid DeviceTokenId { get; set; }
    public string Token { get; set; } = default!;
    public NotificationStatus Status { get; set; }
    public int AttemptNumber {get; set;}
    public DateTime? SentAt { get; set; }
    public string? Error { get; set; }
    public string? FcmMessageId { get; set; }
    
    public OutboxNotification OutboxNotification { get; set; } = default!;
}
}