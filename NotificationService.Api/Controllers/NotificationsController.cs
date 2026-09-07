using Microsoft.AspNetCore.Mvc;
using NotificationService.Core.Dtos.Delivery;
using NotificationService.Core.Dtos.Notification;
using NotificationService.Core.Services;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController(NotificationProcessingService service) : ControllerBase
{
    [HttpPost("broadcast")]
    public async Task<ActionResult<NotificationResponseDto>> Broadcast(
        [FromBody] BroadcastRequestDto request, CancellationToken ct) =>
        Accepted(await service.EnqueueBroadcastAsync(request, ct));

    [HttpPost("send")]
    public async Task<ActionResult<NotificationResponseDto>> SendToUsers(
        [FromBody] SendToUsersRequestDto request, CancellationToken ct) =>
        Accepted(await service.EnqueueToUsersAsync(request, ct));

    [HttpGet("{notificationId:guid}/status")]
    public async Task<ActionResult<NotificationStatusResponseDto>> GetStatus(
        Guid notificationId, CancellationToken ct)
    {
        var response = await service.GetStatusAsync(notificationId, ct);
        if (response is null)
            return NotFound(new { error = "Notification not found or not yet processed." });

        return Ok(response);
    }

    [HttpGet("user/{userId}/history")]
    public async Task<ActionResult<IEnumerable<UserNotificationDto>>> GetUserHistory(
        string userId, string locale = "en", CancellationToken ct = default) =>
        Ok(await service.GetUserHistoryAsync(userId, locale, ct));
}