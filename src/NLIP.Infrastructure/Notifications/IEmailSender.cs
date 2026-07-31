namespace NLIP.Infrastructure.Notifications;

public interface IEmailSender
{
    Task SendAsync(string toAddress, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
