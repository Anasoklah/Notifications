
using NotificationService.Application.DTOs.Delivery;
using NotificationService.Application.DTOs.DeviceToken;
using NotificationService.Application.DTOs.Notification;

namespace NotificationService.Application.Interfaces.Processors;

public interface INotificationProcessor
{
Task ProcessFcmBatchAsync(string workerId, CancellationToken ct = default);
}

