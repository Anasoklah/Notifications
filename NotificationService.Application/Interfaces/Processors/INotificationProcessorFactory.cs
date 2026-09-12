

using NotificationService.Core.Enums;

namespace NotificationService.Application.Interfaces.Processors;

public interface INotificationProcessorFactory
{
    INotificationProcessor GetProcessor(NotificationType type);
}
