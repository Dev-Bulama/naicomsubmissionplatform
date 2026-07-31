using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit;
using NLIP.Application.Common.Interfaces;
using NLIP.Shared.Constants;

namespace NLIP.Infrastructure.Notifications;

public class SmtpEmailSender : IEmailSender
{
    private readonly ISettingsService _settings;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(ISettingsService settings, ILogger<SmtpEmailSender> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task SendAsync(string toAddress, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var host = await _settings.GetAsync(SettingKeys.EmailSmtpHost, cancellationToken);
        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogWarning("NLIP Email: SMTP host not configured; skipping send of '{Subject}' to {To}", subject, toAddress);
            return;
        }

        var port = await _settings.GetIntAsync(SettingKeys.EmailSmtpPort, 587, cancellationToken);
        var from = await _settings.GetAsync(SettingKeys.EmailFromAddress, cancellationToken) ?? "no-reply@nlip.local";

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(from));
        message.To.Add(MailboxAddress.Parse(toAddress));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls, cancellationToken);
            await client.SendAsync(message, cancellationToken);
        }
        finally
        {
            if (client.IsConnected)
                await client.DisconnectAsync(true, cancellationToken);
        }
    }
}
