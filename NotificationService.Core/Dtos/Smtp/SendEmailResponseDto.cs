
namespace NotificationService.Core.Dtos.Smtp;

public record SendEmailResponseDto
(
    Guid Id,
    string Status,
    DateTime CreatedAt,
    DateTime? ScheduledAt 
);
