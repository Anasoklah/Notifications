using System.Net.Mail;
using NotificationService.Core.Enums;

namespace NotificationService.Core.Entities;

    public class OutboxEmail
    {
        public Guid Id { get; private set; }
        public string ToEmail { get; private set; } = default!;
        public EmailMessageType Type { get; private set; }
        public string PayloadJson { get; private set; } = default!;
        public NotificationStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ProcessedAt { get; private set; }
        public DateTime? LockedAt { get; private set; }
        public string? LockedBy { get; private set; }
        public int RetryCount { get; private set; }
        public int MaxRetries { get; private set; } = 3;
        public string? LastError { get; private set; }
        public DateTime? ScheduledAt { get; private set; }

        private OutboxEmail() { }

        public static OutboxEmail Create(
            string toEmail,
            EmailMessageType type,
            string payloadJson,
            DateTime? scheduledAt = null)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("Email is required.", nameof(toEmail));

            if (toEmail.Length > 200)
                throw new ArgumentException("Email must not exceed 200 characters.", nameof(toEmail));

            try
            {
                if (new MailAddress(toEmail).Address != toEmail)
                    throw new ArgumentException("Email format is invalid.", nameof(toEmail));
            }
            catch (FormatException)
            {
                throw new ArgumentException("Email format is invalid.", nameof(toEmail));
            }

            if (!Enum.IsDefined(type))
                throw new ArgumentException("Email type is invalid.", nameof(type));

            if (string.IsNullOrWhiteSpace(payloadJson))
                throw new ArgumentException("Payload is required.", nameof(payloadJson));

            if (scheduledAt.HasValue && scheduledAt.Value <= DateTime.UtcNow)
                throw new ArgumentException("Scheduled time must be in the future.", nameof(scheduledAt));

            return new OutboxEmail
            {
                Id = Guid.NewGuid(),
                ToEmail = toEmail,
                Type = type,
                PayloadJson = payloadJson,
                Status = NotificationStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ScheduledAt = scheduledAt
            };
        }

        public void MarkProcessing(string workerId, DateTime now)
        {
            if (Status != NotificationStatus.Pending)
                throw new InvalidOperationException("Only pending emails can be processed.");

            if (string.IsNullOrWhiteSpace(workerId))
                throw new ArgumentException("Worker id is required.", nameof(workerId));

            Status = NotificationStatus.Processing;
            LockedAt = now;
            LockedBy = workerId;
        }

        public void MarkProcessed(DateTime now)
        {
            Status = NotificationStatus.Sent;
            ProcessedAt = now;
            LockedAt = null;
            LockedBy = null;
        }

        public void RecordFailure(string error)
        {
            if (string.IsNullOrWhiteSpace(error))
                throw new ArgumentException("Error is required.", nameof(error));

            RetryCount++;
            LastError = error;
            LockedAt = null;
            LockedBy = null;
            Status = RetryCount >= MaxRetries
                ? NotificationStatus.Failed
                : NotificationStatus.Pending;
        }

        public void ReleaseStaleLock()
        {
            if (Status != NotificationStatus.Processing)
                return;

            Status = NotificationStatus.Pending;
            LockedAt = null;
            LockedBy = null;
        }
    }

