using NotificationService.Core.Enums;

namespace NotificationService.Application.DTOs.DeviceToken;

public record RegisterTokenRequestDto
(
    string UserId , 
    string Token , 
    Platform Platform ,
    string Locale = "en"
);
