

using System.Text.Json;
using FluentValidation;
using NotificationService.Application.DTOs.Smtp;
using NotificationService.Application.Interfaces.Smtp;
using NotificationService.Core.Entities;
using NotificationService.Core.Enums;

namespace NotificationService.Application.UseCases.Notifiactions.Smtp;

public class EnqueueEmailUseCase(
    ISmtpRepository repo,
    IValidator<SendEmailRequestDto> Validator)
{
      public async Task<SendEmailResponseDto> EnqueueEmailAsync(
        SendEmailRequestDto dto, EmailMessageType type, CancellationToken ct = default)
    {
        await Validator.ValidateAndThrowAsync(dto, ct);

        var email = new OutboxEmail
        {
            ToEmail = dto.ToEmail,
            PayloadJson = BuildPayload(type, dto.Token),
            Type = type,
            ScheduledAt = dto.ScheduledAt
        };

        await repo.AddOutboxEmailAsync(email, ct);

        return new SendEmailResponseDto(
            email.Id,
            email.Status.ToString(),
            email.CreatedAt,
            email.ScheduledAt);
    }


    private static string BuildPayload(EmailMessageType type, string token) => type switch
    {
        EmailMessageType.ConfirmEmail => JsonSerializer.Serialize(new { ConfirmToken = token }),
        EmailMessageType.ResetPassword => JsonSerializer.Serialize(new { ResetToken = token }),
        EmailMessageType.TwoFactorCode => JsonSerializer.Serialize(new { TwoFactorCode = token }),
        _ => throw new InvalidOperationException($"Unsupported email type: {type}")
    };
}
