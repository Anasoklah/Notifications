using NotificationService.Core.ValueObjects;

namespace NotificationService.Application.DTOs.Notification;

public record BroadcastRequestDto
(
LocalizedContent? Title ,
LocalizedContent? Body ,
Dictionary<string, string>? Data ,
DateTime? ScheduledAt 
);  
