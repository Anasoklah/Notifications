
namespace NotificationService.Core.Dtos.Smtp;

public record SendEmailRequestDto
(
    string ToEmail , 
    string Token , 
    DateTime? ScheduledAt  
);
