

using System.Text.Json;
using NotificationService.Core.Enums;
using NotificationService.Core.ValueObjects;

namespace NotificationService.Core.Entities
{
   
public class OutboxNotification
{
    public Guid Id { get; set; }
    public string? TargetUserId { get; set; }   
    public bool IsBroadcast { get; set; }
    // public string? Title { get; set; }
    // public string? Body { get; set; }

    public string? TitleLocalized {get; set; } = default!;
    public string? BodyLocalized {get; set; } = default!;
    public string? Data { get; set; }             
    public NotificationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? LockedAt { get; set; }
    public string? LockedBy { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; } = 3;
    public string? LastError { get; set; }
    public DateTime? ScheduledAt { get; set; }


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
}
}