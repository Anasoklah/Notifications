using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces.Processors;
using NotificationService.Infrastructure.Processors;

namespace NotificationService.Infrastructure.Workers;

public class SmtpWorker(IServiceScopeFactory scopeFactory, ILogger<SmtpWorker> logger)
    : BackgroundService
{
    private readonly string _workerId = Environment.MachineName;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await ProcessBatchAsync(stoppingToken); }
            catch (Exception ex) { logger.LogError(ex, "SmtpWorker error"); }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEmailProcessor>();
        await service.ProcessEmailBatchAsync(_workerId, ct);
    }
}