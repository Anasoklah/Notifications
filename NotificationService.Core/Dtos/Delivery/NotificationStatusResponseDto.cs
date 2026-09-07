

namespace NotificationService.Core.Dtos.Delivery;

public record NotificationStatusResponseDto
(
     Guid NotificationId ,
     string Status , 
     int RetryCount ,
     string? LastError ,
     DateTime CreatedAt ,
     DateTime? ProcessedAt ,
     List<DeliveryDetailDto> Deliveries
);