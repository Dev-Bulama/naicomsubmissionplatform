using NLIP.Domain.Common;
using NLIP.Domain.Enums;

namespace NLIP.Domain.Entities.Integration;

/// <summary>One row per actual call made to the NAICOM Portal API for a policy — the durable
/// audit record of "what we sent NAICOM and what it said back", kept even after retries succeed.</summary>
public class NaicomTransaction : BaseEntity
{
    public Guid PolicyId { get; set; }
    public NaicomAction Action { get; set; }
    public string? NaicomPolicyId { get; set; }
    public Guid CorrelationId { get; set; }
    public string RequestPayload { get; set; } = default!;
    public string? ResponsePayload { get; set; }
    public int? HttpStatusCode { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public long DurationMs { get; set; }
    public DateTimeOffset AttemptedAt { get; set; } = DateTimeOffset.UtcNow;
}
