using NotificationService.Application.DTOs.Smtp;
using NotificationService.Core.Enums;

namespace NotificationService.Application.Interfaces.Processors;

public interface IEmailProcessor
{
    Task ProcessEmailBatchAsync(string workerId, CancellationToken ct = default);
}

