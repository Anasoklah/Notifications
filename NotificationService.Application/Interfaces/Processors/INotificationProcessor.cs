
using NotificationService.Application.DTOs.Delivery;
using NotificationService.Application.DTOs.DeviceToken;
using NotificationService.Application.DTOs.Notification;

namespace NotificationService.Application.Interfaces.Processors;

public interface INotificationProcessor
{

    Task DeactivateAllUserTokensAsync(string userId, CancellationToken ct = default);
    Task DeactivateTokenAsync(string token, CancellationToken ct = default);
    Task<NotificationResponseDto> EnqueueBroadcastAsync(BroadcastRequestDto request, CancellationToken ct = default);
    Task<NotificationResponseDto> EnqueueToUsersAsync(SendToUsersRequestDto request, CancellationToken ct = default);
    Task<NotificationStatusResponseDto?> GetStatusAsync(Guid notificationId, CancellationToken ct = default);
    Task<IEnumerable<UserNotificationDto>> GetUserHistoryAsync(string userId, string locale = "en", CancellationToken ct = default);
    Task ProcessFcmBatchAsync(string workerId, CancellationToken ct = default);
    Task<RegisterTokenResponseDto> RegisterDeviceTokenAsync(RegisterTokenRequestDto request, CancellationToken ct = default);
}

