using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.DTOs.DeviceToken;
using NotificationService.Application.UseCases.Token;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/device-tokens")]
public class DeviceTokensController(
    RegisterTokenUseCase RegisterUseCase,
    DeActivationTokenUseCase deActivationTokenUseCase) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<RegisterTokenResponseDto>> Register(
        [FromBody] RegisterTokenRequestDto request, CancellationToken ct) =>
        Ok(await RegisterUseCase.RegisterDeviceTokenAsync(request, ct));

    [HttpDelete("deactivate")]
    public async Task<ActionResult> Deactivate(
        [FromBody] DeactivateTokenRequestDto request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return BadRequest(new { error = "Token is required." });

        await deActivationTokenUseCase.DeactivateTokenAsync(request.Token, ct);
        return NoContent();
    }

    [HttpDelete("deactivate/user/{userId}")]
    public async Task<ActionResult> DeactivateAllForUser(string userId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest(new { error = "UserId is required." });

        await deActivationTokenUseCase.DeactivateAllUserTokensAsync(userId, ct);
        return NoContent();
    }
}