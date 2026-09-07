using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.Logging;
using NotificationService.Core.Dtos.Smtp;
using NotificationService.Core.Entities;
using NotificationService.Core.Enums;
using NotificationService.Core.Interfaces;

namespace NotificationService.Core.Services;

public class EmailProcessingService(
    ISmtpRepository repo,
    ISmptService smtp,
    IValidator<SendEmailRequestDto> emailValidator,
    ILogger<EmailProcessingService> logger)
{
    public async Task<SendEmailResponseDto> EnqueueEmailAsync(
        SendEmailRequestDto dto, EmailMessageType type, CancellationToken ct = default)
    {
        await emailValidator.ValidateAndThrowAsync(dto, ct);

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

    public async Task ProcessEmailBatchAsync(string workerId, CancellationToken ct = default)
    {
        await repo.ReleaseStaleEmailLockAsync(TimeSpan.FromMinutes(5), ct);

        var batch = await repo.LockPendingEmailBatchAsync(50, workerId, ct);
        if (batch.Count == 0) return;

        foreach (var email in batch)
        {
            try
            {
                await smtp.SendByTypeAsync(email.Type, email.ToEmail, email.PayloadJson, ct);

                await repo.AddEmailDeliveriesAsync(
                    [new EmailDelivery
                    {
                        OutboxEmailId = email.Id,
                        Status = NotificationStatus.Sent,
                        SentAt = DateTime.UtcNow,
                        AttemptNumber = email.RetryCount + 1
                    }],
                    ct);

                await repo.MarkEmailAsProcessedAsync(email.Id, ct);
            }
            catch (Exception ex)
            {
                await repo.AddEmailDeliveriesAsync(
                    [new EmailDelivery
                    {
                        OutboxEmailId = email.Id,
                        Status = NotificationStatus.Failed,
                        Error = ex.Message,
                        AttemptNumber = email.RetryCount + 1
                    }],
                    ct);

                await repo.IncrementEmailRetryAsync(email.Id, ex.Message, ct);
                logger.LogWarning(ex, "Failed smtp outbox email {EmailId}", email.Id);
            }
        }
    }

    private static string BuildPayload(EmailMessageType type, string token) => type switch
    {
        EmailMessageType.ConfirmEmail => JsonSerializer.Serialize(new { ConfirmToken = token }),
        EmailMessageType.ResetPassword => JsonSerializer.Serialize(new { ResetToken = token }),
        EmailMessageType.TwoFactorCode => JsonSerializer.Serialize(new { TwoFactorCode = token }),
        _ => throw new InvalidOperationException($"Unsupported email type: {type}")
    };
}