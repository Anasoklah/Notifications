
using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces.Processors;
using NotificationService.Application.Interfaces.Smtp;
using NotificationService.Core.Entities;
using NotificationService.Core.Enums;

namespace NotificationService.Infrastructure.Processors;


public class SmtpProcessor(
    ISmtpRepository repo,
    ISmptService smtp,
    ILogger<SmtpProcessor> logger) : INotificationProcessor
{
    public NotificationType Type => NotificationType.Smtp;

    public async Task ProcessAsync(string workerId, CancellationToken ct)
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
                    [EmailDelivery.Create(
                        email.Id,
                        NotificationStatus.Sent,
                        email.RetryCount + 1,
                        DateTime.UtcNow)],
                    ct);

                await repo.MarkEmailAsProcessedAsync(email.Id, ct);
            }
            catch (Exception ex)
            {
                await repo.AddEmailDeliveriesAsync(
                    [EmailDelivery.Create(
                        email.Id,
                        NotificationStatus.Failed,
                        email.RetryCount + 1,
                        error: ex.Message)],
                    ct);

                await repo.IncrementEmailRetryAsync(email.Id, ex.Message, ct);
                logger.LogWarning(ex, "Failed smtp outbox email {EmailId}", email.Id);
            }
        }
    }
}