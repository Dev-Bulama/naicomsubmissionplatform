using NLIP.Domain.Common;
using NLIP.Domain.Enums;

namespace NLIP.Domain.Entities.Policies;

/// <summary>Immutable status-change ledger for a policy, independent of the NAICOM transaction log.</summary>
public class PolicyHistory : BaseEntity
{
    public Guid PolicyId { get; private set; }
    public Policy? Policy { get; private set; }
    public PolicyStatus PreviousStatus { get; private set; }
    public PolicyStatus NewStatus { get; private set; }
    public string? Notes { get; private set; }
    public DateTimeOffset ChangedAt { get; private set; } = DateTimeOffset.UtcNow;

    private PolicyHistory() { }

    internal PolicyHistory(Guid policyId, PolicyStatus previousStatus, PolicyStatus newStatus, string? notes)
    {
        PolicyId = policyId;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        Notes = notes;
    }
}
