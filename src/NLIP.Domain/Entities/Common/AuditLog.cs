using NLIP.Domain.Common;

namespace NLIP.Domain.Entities.Common;

/// <summary>Compliance-grade audit trail: who did what to which entity, with before/after detail.
/// Distinct from ActivityLog, which is a lighter, non-compliance user-activity feed.</summary>
public class AuditLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = default!;
    public string EntityName { get; set; } = default!;
    public string? EntityId { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
