using NLIP.Domain.Enums;

namespace NLIP.Application.Common.Interfaces;

/// <summary>Fan-out notification sender. Implemented in Infrastructure, dispatching to SignalR,
/// email, and (stubbed, "ready") SMS/Teams/Slack based on <see cref="NotificationChannel"/>.</summary>
public interface INotificationService
{
    Task NotifyUserAsync(Guid userId, string title, string message, NotificationChannel channel, Guid? relatedPolicyId = null, CancellationToken cancellationToken = default);
    Task BroadcastAsync(string title, string message, NotificationChannel channel, Guid? relatedPolicyId = null, CancellationToken cancellationToken = default);
}
