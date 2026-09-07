namespace NotificationService.Core.Dtos.Delivery;

public record DeliveryDetailDto
(
    Guid DeliveryId ,
    string Token , 
    string Status , 
    int AttemptNumber ,
    DateTime? SentAt ,
    string? FcmMessageId ,
    string? Error 
);
