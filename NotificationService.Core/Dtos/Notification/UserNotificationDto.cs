using System;
using NotificationService.Core.ValueObjects;

namespace NotificationService.Core.Dtos.Notification;

public record UserNotificationDto
(
Guid NotificationId ,
string? Title ,
string? Body ,
Dictionary<string, string>? Data, 
bool IsBroadcast ,
string Status ,
DateTime CreatedAt ,
DateTime? ProcessedAt 
);
