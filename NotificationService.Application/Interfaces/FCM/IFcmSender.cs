namespace NotificationService.Application.Interfaces.FCM
{
public interface IFcmSender
{
    Task<(bool Success, string? MessageId, string? Error)> SendAsync(
        string token, string? title, string? body, string? data, CancellationToken ct);
}
}