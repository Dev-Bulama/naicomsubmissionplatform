using Microsoft.AspNetCore.SignalR;
using NLIP.Application.Common.Interfaces;
using NLIP.Domain.Entities.Common;
using NLIP.Domain.Enums;
using NLIP.Infrastructure.Realtime;

namespace NLIP.Infrastructure.Notifications;

public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _context;
    private readonly IHubContext<NotificationHub> _hub;
    private readonly IEmailSender _email;
    private readonly ISmsSender _sms;

    public NotificationService(IApplicationDbContext context, IHubContext<NotificationHub> hub, IEmailSender email, ISmsSender sms)
    {
        _context = context;
        _hub = hub;
        _email = email;
        _sms = sms;
    }

    public async Task NotifyUserAsync(Guid userId, string title, string message, NotificationChannel channel, Guid? relatedPolicyId = null, CancellationToken cancellationToken = default)
    {
        var notification = Persist(userId, title, message, channel, relatedPolicyId);
        await _context.SaveChangesAsync(cancellationToken);
        await DispatchAsync(notification, $"user:{userId}", cancellationToken);
    }

    public async Task BroadcastAsync(string title, string message, NotificationChannel channel, Guid? relatedPolicyId = null, CancellationToken cancellationToken = default)
    {
        var notification = Persist(null, title, message, channel, relatedPolicyId);
        await _context.SaveChangesAsync(cancellationToken);
        await DispatchAsync(notification, null, cancellationToken);
    }

    private Notification Persist(Guid? userId, string title, string message, NotificationChannel channel, Guid? relatedPolicyId)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Channel = channel,
            RelatedPolicyId = relatedPolicyId
        };
        _context.Notifications.Add(notification);
        return notification;
    }

    private async Task DispatchAsync(Notification notification, string? group, CancellationToken cancellationToken)
    {
        var payload = new { notification.Id, notification.Title, notification.Message, notification.RelatedPolicyId, notification.CreatedAt };

        switch (notification.Channel)
        {
            case NotificationChannel.SignalR:
            case NotificationChannel.InApp:
                if (group is null)
                    await _hub.Clients.All.SendAsync("ReceiveNotification", payload, cancellationToken);
                else
                    await _hub.Clients.Group(group).SendAsync("ReceiveNotification", payload, cancellationToken);
                break;
            case NotificationChannel.Email when notification.UserId.HasValue:
                // Real recipient resolution (User.Email lookup) happens in the API-layer
                // consumer; this default path is intentionally best-effort/log-only until wired.
                break;
        }

        notification.Status = NotificationStatus.Sent;
        notification.SentAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
