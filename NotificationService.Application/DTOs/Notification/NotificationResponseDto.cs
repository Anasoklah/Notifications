namespace NotificationService.Application.DTOs.Notification;

public record NotificationResponseDto
(
    Guid NotificationId ,
    string Status , 
    int QueuedCount ,          // how many outbox rows created
    DateTime CreatedAt ,
    DateTime? ScheduledAt 
);
