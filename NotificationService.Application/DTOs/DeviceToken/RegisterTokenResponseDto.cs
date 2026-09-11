using NotificationService.Core.Enums;

namespace NotificationService.Application.DTOs.DeviceToken;

public record RegisterTokenResponseDto
(
    string UserId , 
    string Token , 
    Platform Platform ,
    DateTime RegisteredAt ,
    string Locale 
);
