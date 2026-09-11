namespace NotificationService.Application.DTOs.Smtp;

public record SendEmailResponseDto
(
    Guid Id,
    string Status,
    DateTime CreatedAt,
    DateTime? ScheduledAt 
);
