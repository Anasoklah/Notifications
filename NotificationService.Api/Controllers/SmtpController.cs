using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.DTOs.Smtp;
using NotificationService.Application.UseCases.Notifiactions.Smtp;
using NotificationService.Application.UseCases.Token;
using NotificationService.Core.Enums;
using NotificationService.Infrastructure.Processors;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SmtpController(
    EnqueueEmailUseCase emailUseCase) : ControllerBase
{
    [HttpPost("send-confirm-email")]
    public async Task<ActionResult<SendEmailResponseDto>> SendConfirmEmail(
        [FromBody] SendEmailRequestDto dto, CancellationToken ct) =>
        Accepted(await emailUseCase.EnqueueEmailAsync(dto, EmailMessageType.ConfirmEmail, ct));

    [HttpPost("send-reset-password")]
    public async Task<ActionResult<SendEmailResponseDto>> SendResetPasswordEmail(
        [FromBody] SendEmailRequestDto dto, CancellationToken ct) =>
        Accepted(await emailUseCase.EnqueueEmailAsync(dto, EmailMessageType.ResetPassword, ct));

    [HttpPost("send-2FA")]
    public async Task<ActionResult<SendEmailResponseDto>> Send2FAEmail(
        [FromBody] SendEmailRequestDto dto, CancellationToken ct) =>
        Accepted(await emailUseCase.EnqueueEmailAsync(dto, EmailMessageType.TwoFactorCode, ct));
}