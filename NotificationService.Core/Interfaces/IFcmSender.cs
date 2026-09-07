
namespace NotificationService.Core.Interfaces
{
public interface IFcmSender
{
    Task<(bool Success, string? MessageId, string? Error)> SendAsync(
        string token, string? title, string? body, string? data, CancellationToken ct);
}
}