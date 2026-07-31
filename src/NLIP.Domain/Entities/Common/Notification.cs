using NLIP.Domain.Common;
using NLIP.Domain.Enums;

namespace NLIP.Domain.Entities.Common;

public class Notification : BaseEntity
{
    /// <summary>Null = broadcast to all users with DashboardView permission (e.g. "NAICOM API unavailable").</summary>
    public Guid? UserId { get; set; }
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public NotificationChannel Channel { get; set; }
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public Guid? RelatedPolicyId { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? SentAt { get; set; }
}
