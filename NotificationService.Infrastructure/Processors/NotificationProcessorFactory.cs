using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NotificationService.Application.Interfaces.Processors;
using NotificationService.Core.Enums;

namespace NotificationService.Infrastructure.Processors;

public class NotificationProcessorFactory : INotificationProcessorFactory
{
    private readonly IEnumerable<INotificationProcessor> _processors;

    public NotificationProcessorFactory(IEnumerable<INotificationProcessor> processors)
    {
        _processors = processors;
    }
    public INotificationProcessor GetProcessor(NotificationType type)
    {
        var processor = _processors.FirstOrDefault(p => p.Type == type);
        return processor ?? throw new NotSupportedException($"No processor registered for type: {type}");
    }
}
