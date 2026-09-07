namespace NotificationService.Infrastructure.Services.Smtp;

public class SmtpOptions
{
    public string Host { get; init; } = default!;
    public int Port { get; init; }
    public bool UseSsl { get; init; }
    public string Username { get; init; } = default!;
    public string Password { get; init; } = default!;
    public string SenderName { get; init; } = default!;
    public string SenderEmail { get; init; } = default!;
    public string BaseUrl { get; init; } = default!;
}
