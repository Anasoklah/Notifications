

using System.Text.Json;
using NotificationService.Application.DTOs.Smtp;
using NotificationService.Application.Interfaces.Smtp;
using NotificationService.Core.Entities;
using NotificationService.Core.Enums;

namespace NotificationService.Application.UseCases.Notifiactions.Smtp;

public class EnqueueEmailUseCase(ISmtpRepository repo)
{
      public async Task<SendEmailResponseDto> EnqueueEmailAsync(
        SendEmailRequestDto dto, EmailMessageType type, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var email = OutboxEmail.Create(
            dto.ToEmail,
            type,
            BuildPayload(type, dto.Token),
            dto.ScheduledAt);

        await repo.AddOutboxEmailAsync(email, ct);

        return new SendEmailResponseDto(
            email.Id,
            email.Status.ToString(),
            email.CreatedAt,
            email.ScheduledAt);
    }

#region Helpers
    private static string BuildPayload(EmailMessageType type, string token) => type switch
    {
        EmailMessageType.ConfirmEmail => JsonSerializer.Serialize(new { ConfirmToken = token }),
        EmailMessageType.ResetPassword => JsonSerializer.Serialize(new { ResetToken = token }),
        EmailMessageType.TwoFactorCode => JsonSerializer.Serialize(new { TwoFactorCode = token }),
        _ => throw new InvalidOperationException($"Unsupported email type: {type}")
    };

#endregion
}
