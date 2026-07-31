using NLIP.Domain.Common;

namespace NLIP.Domain.Entities.Common;

/// <summary>Lightweight "recent activity" feed shown on the dashboard (e.g. "J. Doe viewed policy X").</summary>
public class ActivityLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public string Description { get; set; } = default!;
    public string Category { get; set; } = default!;
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
