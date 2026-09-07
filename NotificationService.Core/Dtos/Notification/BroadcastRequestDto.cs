
using NotificationService.Core.ValueObjects;

namespace NotificationService.Core.Dtos.Notification;

public record BroadcastRequestDto
(
LocalizedContent? Title ,
LocalizedContent? Body ,
Dictionary<string, string>? Data ,
DateTime? ScheduledAt 
);  
