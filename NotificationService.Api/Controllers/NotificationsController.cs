using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.DTOs.Delivery;
using NotificationService.Application.DTOs.Notification;
using NotificationService.Application.UseCases.Notifiactions.FCM;
using NotificationService.Infrastructure.Processors;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController(
    EnqueueBroadcastUseCase broadcastUseCase,
    EnqueuToUserUseCase toUserUseCase,
    GetNotificationStatusUseCase statusUseCase,
    GetUserHistoryUseCase userHistoryUseCase) : ControllerBase
{
    [HttpPost("broadcast")]
    public async Task<ActionResult<NotificationResponseDto>> Broadcast(
        [FromBody] BroadcastRequestDto request, CancellationToken ct) =>
        Accepted(await broadcastUseCase.EnqueueBroadcastAsync(request, ct));

    [HttpPost("send")]
    public async Task<ActionResult<NotificationResponseDto>> SendToUsers(
        [FromBody] SendToUsersRequestDto request, CancellationToken ct) =>
        Accepted(await toUserUseCase.EnqueueToUsersAsync(request, ct));

    [HttpGet("{notificationId:guid}/status")]
    public async Task<ActionResult<NotificationStatusResponseDto>> GetStatus(
        Guid notificationId, CancellationToken ct)
    {
        var response = await statusUseCase.GetStatusAsync(notificationId, ct);
        if (response is null)
            return NotFound(new { error = "Notification not found or not yet processed." });

        return Ok(response);
    }

    [HttpGet("user/{userId}/history")]
    public async Task<ActionResult<IEnumerable<UserNotificationDto>>> GetUserHistory(
        string userId, string locale = "en", CancellationToken ct = default) =>
        Ok(await userHistoryUseCase.GetUserHistoryAsync(userId, locale, ct));
}