namespace NotificationService.Application.DTOs.Smtp;

public record SendEmailRequestDto
(
    string ToEmail , 
    string Token , 
    DateTime? ScheduledAt  
);
