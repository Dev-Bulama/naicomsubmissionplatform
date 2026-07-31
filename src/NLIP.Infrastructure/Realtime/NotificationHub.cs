using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace NLIP.Infrastructure.Realtime;

/// <summary>
/// Mapped by the host (API/Web: app.MapHub&lt;NotificationHub&gt;("/hubs/notifications")). Pushes
/// dashboard/toast events for submission success/failure, retry outcomes, and NAICOM
/// availability changes — see INotificationService. Clients join a per-user group on connect so
/// NotifyUserAsync can target one user while BroadcastAsync reaches everyone.
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");

        await base.OnConnectedAsync();
    }
}
