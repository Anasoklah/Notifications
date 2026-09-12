
using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces.Processors;
using NotificationService.Application.Interfaces.Smtp;
using NotificationService.Core.Entities;
using NotificationService.Core.Enums;

namespace NotificationService.Infrastructure.Processors;



public class EmailProcessor(
    ISmtpRepository repo,
    ISmptService smtp,
    ILogger<EmailProcessor> logger) : IEmailProcessor
{

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


}