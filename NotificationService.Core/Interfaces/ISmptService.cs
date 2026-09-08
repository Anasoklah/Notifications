

using NotificationService.Core.Enums;

namespace NotificationService.Core.Interfaces;
public interface ISmptService
{
    Task SendByTypeAsync(EmailMessageType type, string toEmail, string payloadJson, CancellationToken ct = default);
    
}
