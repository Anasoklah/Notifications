

using NotificationService.Core.Enums;

namespace NotificationService.Core.Interfaces;
public interface ISmptService
{
    Task SendByTypeAsync(EmailMessageType type, string toEmail, string payloadJson, CancellationToken ct = default);
    Task SendConfirmationEmail(string email, string confirmToken, CancellationToken ct = default);
    Task SendResetPasswordEmail(string email, string resetToken, CancellationToken ct = default);
    Task SendTwoFactorCodeEmail(string email, string twoFactorcode, CancellationToken ct = default);
}
