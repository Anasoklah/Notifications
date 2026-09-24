

using System.Text.Json;
using NotificationService.Core.Enums;
using NotificationService.Core.ValueObjects;

namespace NotificationService.Core.Entities
{
   
public class OutboxNotification
{
    public Guid Id { get; private set; }
    public string? TargetUserId { get; private set; }
    public bool IsBroadcast { get; private set; }
    public string? TitleLocalized { get; private set; }
    public string? BodyLocalized { get; private set; }
    public string? Data { get; private set; }
    public NotificationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public DateTime? LockedAt { get; private set; }
    public string? LockedBy { get; private set; }
    public int RetryCount { get; private set; }
    public int MaxRetries { get; private set; } = 3;
    public string? LastError { get; private set; }
    public DateTime? ScheduledAt { get; private set; }

    private OutboxNotification() { }

    public static OutboxNotification CreateBroadcast(
        string? titleLocalized,
        string? bodyLocalized,
        string? data,
        DateTime? scheduledAt = null) =>
        Create(null, true, titleLocalized, bodyLocalized, data, scheduledAt);

    public static OutboxNotification CreateForUser(
        string userId,
        string? titleLocalized,
        string? bodyLocalized,
        string? data,
        DateTime? scheduledAt = null) =>
        Create(userId, false, titleLocalized, bodyLocalized, data, scheduledAt);

    public void MarkProcessing(string workerId, DateTime now)
    {
        if (Status != NotificationStatus.Pending)
            throw new InvalidOperationException("Only pending notifications can be processed.");

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


    public string? ResolcveTitle(string locale)
    {

        if(TitleLocalized is null) return null;

            var map = JsonSerializer.Deserialize<LocalizedContent>(TitleLocalized);

           return  map?.Resolve(locale);
    }

    public string? ResolcveBody(string locale)
    {
        if(BodyLocalized is null) return null;

            var map = JsonSerializer.Deserialize<LocalizedContent>(BodyLocalized);

           return  map?.Resolve(locale);
    }

    private static OutboxNotification Create(
        string? targetUserId,
        bool isBroadcast,
        string? titleLocalized,
        string? bodyLocalized,
        string? data,
        DateTime? scheduledAt)
    {
        if (!isBroadcast && string.IsNullOrWhiteSpace(targetUserId))
            throw new ArgumentException("Target user id is required.", nameof(targetUserId));

        if (titleLocalized is null && bodyLocalized is null)
            throw new ArgumentException("At least a title or body is required.");

        if (scheduledAt.HasValue && scheduledAt.Value <= DateTime.UtcNow)
            throw new ArgumentException("Scheduled time must be in the future.", nameof(scheduledAt));

        return new OutboxNotification
        {
            Id = Guid.NewGuid(),
            TargetUserId = targetUserId,
            IsBroadcast = isBroadcast,
            TitleLocalized = titleLocalized,
            BodyLocalized = bodyLocalized,
            Data = data,
            Status = NotificationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ScheduledAt = scheduledAt ?? DateTime.UtcNow
        };
    }
}
}