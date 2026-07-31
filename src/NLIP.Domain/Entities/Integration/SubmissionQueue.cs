using NLIP.Domain.Common;
using NLIP.Domain.Enums;

namespace NLIP.Domain.Entities.Integration;

/// <summary>
/// Outbox row: written in the same transaction as the domain-entity change that caused it
/// (see DispatchDomainEventsInterceptor), then picked up by the Hangfire outbox processor and
/// dispatched to NAICOM. Also serves as the retry/dead-letter queue — RetryCount/NextAttemptAt/
/// Status are updated in place rather than using a second physical table, since a "RetryQueue"
/// is just a SubmissionQueue row that failed at least once.
/// </summary>
public class SubmissionQueue : BaseEntity
{
    public Guid PolicyId { get; set; }
    public NaicomAction Action { get; set; }

    /// <summary>JSON snapshot of the policy (+ children) at enqueue time, so a retry sends the
    /// data as it was when queued, not whatever the policy has mutated to since.</summary>
    public string Payload { get; set; } = default!;

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Queued;
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; } = 6;
    public DateTimeOffset? NextAttemptAt { get; set; }
    public DateTimeOffset? LastAttemptAt { get; set; }
    public string? LastError { get; set; }
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public string? HangfireJobId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProcessedAt { get; set; }

    /// <summary>Standard exponential-ish backoff schedule (seconds) applied by the retry engine;
    /// index = RetryCount - 1, clamped to the last entry once exhausted-but-under MaxRetries.</summary>
    public static readonly int[] RetryScheduleSeconds = { 30, 60, 300, 900, 1800, 3600 };
}
