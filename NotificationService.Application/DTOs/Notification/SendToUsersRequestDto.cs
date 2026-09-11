using NotificationService.Core.ValueObjects;

namespace NotificationService.Application.DTOs.Notification;

public record SendToUsersRequestDto
(
    List<string> UserIds , 
    LocalizedContent? Title, 
    LocalizedContent? Body ,
    Dictionary<string, string>? Data, 
    DateTime? ScheduledAt 
);
