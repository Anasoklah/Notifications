using NotificationService.Core.Enums;

namespace NotificationService.Application.Interfaces.Smtp;
public interface ISmptService
{
    Task SendByTypeAsync(EmailMessageType type, string toEmail, string payloadJson, CancellationToken ct = default);
    
}
