using Microsoft.AspNetCore.Mvc;
using NotificationService.Core.Dtos.Smtp;
using NotificationService.Core.Enums;
using NotificationService.Core.Services;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SmtpController(EmailProcessingService service) : ControllerBase
{
    [HttpPost("send-confirm-email")]
    public async Task<ActionResult<SendEmailResponseDto>> SendConfirmEmail(
        [FromBody] SendEmailRequestDto dto, CancellationToken ct) =>
        Accepted(await service.EnqueueEmailAsync(dto, EmailMessageType.ConfirmEmail, ct));

    [HttpPost("send-reset-password")]
    public async Task<ActionResult<SendEmailResponseDto>> SendResetPasswordEmail(
        [FromBody] SendEmailRequestDto dto, CancellationToken ct) =>
        Accepted(await service.EnqueueEmailAsync(dto, EmailMessageType.ResetPassword, ct));

    [HttpPost("send-2FA")]
    public async Task<ActionResult<SendEmailResponseDto>> Send2FAEmail(
        [FromBody] SendEmailRequestDto dto, CancellationToken ct) =>
        Accepted(await service.EnqueueEmailAsync(dto, EmailMessageType.TwoFactorCode, ct));
}