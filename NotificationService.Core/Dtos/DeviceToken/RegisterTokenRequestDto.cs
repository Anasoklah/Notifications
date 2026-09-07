
using NotificationService.Core.Enums;

namespace NotificationService.Core.Dtos.DeviceToken;

public record RegisterTokenRequestDto
(
    string UserId , 
    string Token , 
    Platform Platform ,
    string Locale = "en"
);
