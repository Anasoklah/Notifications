

namespace NotificationService.Application.DTOs.Notification;

public record UserNotificationDto
(
Guid NotificationId ,
string? Title ,
string? Body ,
Dictionary<string, string>? Data, 
bool IsBroadcast ,
string Status ,
DateTime CreatedAt ,
DateTime? ProcessedAt 
);
