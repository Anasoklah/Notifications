using System.Text;
using System.Text.Json;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Encodings;
using NotificationService.Core.Enums;
using NotificationService.Core.Interfaces;

namespace NotificationService.Infrastructure.Services.Smtp;

public class SmptService : ISmptService
{
    private readonly SmtpOptions _settings;


    public SmptService(IOptions<SmtpOptions> options)
    {
        this._settings = options.Value;
    }

    #region Public 
    public async Task SendByTypeAsync(
    EmailMessageType type,
    string toEmail,
    string payloadJson,
    CancellationToken ct = default)
    {
        switch (type)
        {
            case EmailMessageType.ConfirmEmail:
            {
                var p = JsonSerializer.Deserialize<ConfirmEmailPayload>(payloadJson)
                        ?? throw new InvalidOperationException("Invalid ConfirmEmail payload.");
                await SendConfirmationEmail(toEmail, p.ConfirmToken, ct);
                break;
            }
            case EmailMessageType.ResetPassword:
            {
                var p = JsonSerializer.Deserialize<ResetPasswordPayload>(payloadJson)
                        ?? throw new InvalidOperationException("Invalid ResetPassword payload.");
                await SendResetPasswordEmail(toEmail, p.ResetToken, ct);
                break;
            }
            case EmailMessageType.TwoFactorCode:
            {
                var p = JsonSerializer.Deserialize<TwoFactorPayload>(payloadJson)
                        ?? throw new InvalidOperationException("Invalid TwoFactor payload.");
                await SendTwoFactorCodeEmail(toEmail, p.TwoFactorCode, ct);
                break;
            }
            default:
                throw new InvalidOperationException($"Unsupported email type: {type}");
        }
    }
#endregion

#region Private helpers
    private async Task SendConfirmationEmail(
        string email,
        string confirmToken,
        CancellationToken ct = default
    )
    {
        var encodedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(confirmToken));

         var url = $"{_settings.BaseUrl}/Auth/VerifyEmail?email={email}&verifyEmailToken={encodedToken}";
        var body = $"<p>Hello {email},</p><p>Confirm here: <a href='{url}'>{url}</a></p>";

        await SendEmailAsync(email, "Confirm your email", body, ct);
    }

    private async Task SendResetPasswordEmail(
        string email,
        string resestToken,
        CancellationToken ct = default
    )
    {
        var encodedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(resestToken));

         var url = $"{_settings.BaseUrl}/Auth/resetpassword?email={email}&ResetToken={encodedToken}";
        var body = $"<p>Hello {email},</p><p>Confirm here: <a href='{url}'>{url}</a></p>"; 

        await SendEmailAsync(
            email,
            "Reset your password",
            body,
            ct);
    }

    private async Task SendTwoFactorCodeEmail(
        string email,
        string code,
        CancellationToken ct = default)
    {
         
         var body = $"<p>Hello {email},</p><p>Your Code is : {code}</p>"; 

        await SendEmailAsync(
            email,
            "Your 2FA Verification Code",
            body,
            ct);
    }

    
    private async Task SendEmailAsync(
        string email,
        string subject,
        string htmlMessage,
        CancellationToken ct = default
    )
    {
        var message = BuildMessage(email, subject, htmlMessage);

        using var client = new SmtpClient();

        try
        {
            await client.ConnectAsync(
                _settings.Host,
                _settings.Port,
                _settings.UseSsl,
                ct
            );

            await client.AuthenticateAsync(
                _settings.Username,
                _settings.Password,
                ct
            );

            await client.SendAsync(message, ct);
        }
        catch (SmtpProtocolException ex)
        {
            throw new InvalidOperationException($"Smtp protocol error: {ex.Message}", ex);
        }
        catch (SmtpCommandException ex)
        {
            throw new InvalidOperationException($"SMTP protocol error: {ex.Message}", ex);
        }
        finally
        {
            if (client.IsConnected)
                await client.DisconnectAsync(true, ct);
        }
    }

    private MimeMessage BuildMessage(
        string email,
        string subject,
        string htmlMessage
    )
    {
        MimeMessage message = new MimeMessage();

        message.From.Add(new MailboxAddress(
            name: _settings.SenderName,
            address: _settings.SenderEmail
        ));

        message.To.Add(MailboxAddress.Parse(email));

        message.Subject = subject.Trim();

        message.Body = new BodyBuilder
        {
            HtmlBody = htmlMessage
        }.ToMessageBody();

        return message;
    }
#endregion



#region Payload DTOs
    public sealed class ConfirmEmailPayload
    {
        public string ConfirmToken { get; set; } = default!;
    }

    public sealed class ResetPasswordPayload
    {
        public string ResetToken { get; set; } = default!;
    }

    public sealed class TwoFactorPayload
    {
        public string TwoFactorCode { get; set; } = default!;
    }

#endregion
}