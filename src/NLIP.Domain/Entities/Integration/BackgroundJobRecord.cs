using NLIP.Domain.Common;

namespace NLIP.Domain.Entities.Integration;

/// <summary>Mirrors a Hangfire job for reporting purposes (Hangfire's own storage is the source
/// of truth for execution; this table lets the dashboard query job history without hitting Hangfire's schema directly).</summary>
public class BackgroundJobRecord : BaseEntity
{
    public string HangfireJobId { get; set; } = default!;
    public string JobType { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTimeOffset EnqueuedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? Error { get; set; }
}
