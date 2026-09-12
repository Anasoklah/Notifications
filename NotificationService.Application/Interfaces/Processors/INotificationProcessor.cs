

using NotificationService.Core.Enums;

namespace NotificationService.Application.Interfaces.Processors;

public interface INotificationProcessor
{
    NotificationType Type {get;}
    public Task ProcessAsync(string workerId , CancellationToken cancellationToken);
}
