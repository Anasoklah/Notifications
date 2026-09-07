using NotificationService.Core.Enums;

namespace NotificationService.Core.Entities
{
    public class OutboxEmail
    {
        public Guid Id { get; set; }
        public string ToEmail { get; set; } = default!;
        public EmailMessageType Type { get; set; }
        public string PayloadJson { get; set; } = default!;

        public NotificationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? LockedAt { get; set; }
        public string? LockedBy { get; set; }
        public int RetryCount { get; set; }
        public int MaxRetries { get; set; } = 3;
        public string? LastError { get; set; }
        public DateTime? ScheduledAt { get; set; }
    }
}
