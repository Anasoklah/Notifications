using NotificationService.Core.Enums;

namespace NotificationService.Core.Dtos.DeviceToken;

public record RegisterTokenResponseDto
(
    string UserId , 
    string Token , 
    Platform Platform ,
    DateTime RegisteredAt ,
    string Locale 
);
