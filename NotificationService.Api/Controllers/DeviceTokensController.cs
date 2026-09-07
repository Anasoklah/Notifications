using Microsoft.AspNetCore.Mvc;
using NotificationService.Core.Dtos.DeviceToken;
using NotificationService.Core.Services;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/device-tokens")]
public class DeviceTokensController(NotificationProcessingService service) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<RegisterTokenResponseDto>> Register(
        [FromBody] RegisterTokenRequestDto request, CancellationToken ct) =>
        Ok(await service.RegisterDeviceTokenAsync(request, ct));

    [HttpDelete("deactivate")]
    public async Task<ActionResult> Deactivate(
        [FromBody] DeactivateTokenRequestDto request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return BadRequest(new { error = "Token is required." });

        await service.DeactivateTokenAsync(request.Token, ct);
        return NoContent();
    }

    [HttpDelete("deactivate/user/{userId}")]
    public async Task<ActionResult> DeactivateAllForUser(string userId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest(new { error = "UserId is required." });

        await service.DeactivateAllUserTokensAsync(userId, ct);
        return NoContent();
    }
}