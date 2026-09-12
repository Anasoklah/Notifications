using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces.Processors;
using NotificationService.Core.Enums;

namespace NotificationService.Infrastructure.Workers;

public class FCMWorker(IServiceScopeFactory scopeFactory, ILogger<FCMWorker> logger)
    : BackgroundService
{
    private readonly string _workerId = Environment.MachineName;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await ProcessBatchAsync(stoppingToken); }
            catch (Exception ex) { logger.LogError(ex, "OutboxWorker error"); }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var Factory = scope.ServiceProvider.GetRequiredService<INotificationProcessorFactory>();
        var FCMProcessor = Factory.GetProcessor(NotificationType.FCM);
        await FCMProcessor.ProcessAsync(_workerId , ct);
    }
}