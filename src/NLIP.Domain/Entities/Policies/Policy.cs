using NLIP.Domain.Common;
using NLIP.Domain.Entities.MasterData;
using NLIP.Domain.Enums;
using NLIP.Domain.Events;
using NLIP.Domain.Exceptions;

namespace NLIP.Domain.Entities.Policies;

/// <summary>
/// Aggregate root for a Life Assurance policy. A single table backs both Individual Life and
/// Group Life (discriminated by <see cref="BusinessType"/>) because the two share the entire
/// submission/audit/retry lifecycle and differ only in their child collection (Beneficiaries vs
/// GroupMembers) and in which NAICOM DTO they map to. This avoids duplicating the outbox/retry/
/// audit plumbing across two near-identical aggregates.
///
/// The Core Insurance Application is always the system of record: CorePolicyId is the immutable
/// foreign key back to it, and every mutation here should originate from a Core event, not from
/// a user typing into this platform (the UI writes only reach the Core app's own APIs — this
/// platform's job is one-way synchronization outward to NAICOM).
/// </summary>
public class Policy : BaseAuditableEntity
{
    public string PolicyNumber { get; private set; } = default!;
    public string CorePolicyId { get; private set; } = default!;
    public string? NaicomPolicyId { get; private set; }
    public BusinessType BusinessType { get; private set; }
    public PolicyStatus Status { get; private set; } = PolicyStatus.Draft;

    public Guid ProductId { get; private set; }
    public Product? Product { get; private set; }
    public Guid BranchId { get; private set; }
    public Branch? Branch { get; private set; }
    public Guid? AgentId { get; private set; }
    public Agent? Agent { get; private set; }

    /// <summary>Insured individual — populated for Individual Life only.</summary>
    public Guid? CustomerId { get; private set; }
    public Customer? Customer { get; private set; }

    /// <summary>Master policyholder — populated for Group Life only.</summary>
    public Guid? EmployerId { get; private set; }
    public Employer? Employer { get; private set; }

    public decimal SumAssured { get; private set; }
    public decimal PremiumAmount { get; private set; }
    public string PremiumFrequency { get; private set; } = "Annual";
    public DateTime CoverageStartDate { get; private set; }
    public DateTime CoverageEndDate { get; private set; }

    public SubmissionStatus CurrentSubmissionStatus { get; private set; } = SubmissionStatus.Queued;
    public int RetryCount { get; private set; }
    public DateTimeOffset? LastSubmittedAt { get; private set; }

    /// <summary>Optimistic concurrency token — required because both the Core-event handler and
    /// the background retry engine can attempt to update the same policy concurrently.</summary>
    public byte[]? RowVersion { get; private set; }

    private readonly List<PolicyBeneficiary> _beneficiaries = new();
    public IReadOnlyCollection<PolicyBeneficiary> Beneficiaries => _beneficiaries.AsReadOnly();

    private readonly List<GroupMember> _groupMembers = new();
    public IReadOnlyCollection<GroupMember> GroupMembers => _groupMembers.AsReadOnly();

    private readonly List<PolicyHistory> _history = new();
    public IReadOnlyCollection<PolicyHistory> History => _history.AsReadOnly();

    private Policy() { }

    public static Policy CreateDraft(
        string policyNumber, string corePolicyId, BusinessType businessType, Guid productId, Guid branchId,
        decimal sumAssured, decimal premiumAmount, string premiumFrequency,
        DateTime coverageStartDate, DateTime coverageEndDate, Guid? agentId,
        Guid? customerId, Guid? employerId)
    {
        if (businessType == BusinessType.IndividualLife && customerId is null)
            throw new DomainException("Individual Life policies require a CustomerId.");
        if (businessType == BusinessType.GroupLife && employerId is null)
            throw new DomainException("Group Life policies require an EmployerId.");
        if (coverageEndDate <= coverageStartDate)
            throw new DomainException("CoverageEndDate must be after CoverageStartDate.");

        var policy = new Policy
        {
            PolicyNumber = policyNumber,
            CorePolicyId = corePolicyId,
            BusinessType = businessType,
            ProductId = productId,
            BranchId = branchId,
            AgentId = agentId,
            CustomerId = customerId,
            EmployerId = employerId,
            SumAssured = sumAssured,
            PremiumAmount = premiumAmount,
            PremiumFrequency = premiumFrequency,
            CoverageStartDate = coverageStartDate,
            CoverageEndDate = coverageEndDate,
            Status = PolicyStatus.Draft
        };
        return policy;
    }

    public void AddBeneficiary(string fullName, string relationship, DateTime dateOfBirth, decimal sharePercentage, string? phone, string? email)
    {
        if (BusinessType != BusinessType.IndividualLife)
            throw new DomainException("Beneficiaries only apply to Individual Life policies.");
        _beneficiaries.Add(new PolicyBeneficiary(Id, fullName, relationship, dateOfBirth, sharePercentage, phone, email));
    }

    public void AddGroupMember(string employeeId, string fullName, DateTime dateOfBirth, string gender, decimal sumAssured, string? designation, DateTime dateJoined)
    {
        if (BusinessType != BusinessType.GroupLife)
            throw new DomainException("Group members only apply to Group Life policies.");
        _groupMembers.Add(new GroupMember(Id, employeeId, fullName, dateOfBirth, gender, sumAssured, designation, dateJoined));
    }

    /// <summary>Marks the policy Active in the Core Application and queues a Create submission to NAICOM.</summary>
    public void Activate()
    {
        if (Status != PolicyStatus.Draft)
            throw new DomainException($"Only Draft policies can be activated (current status: {Status}).");

        RecordHistory(Status, PolicyStatus.Active, "Policy activated");
        Status = PolicyStatus.Active;
        AddDomainEvent(new PolicyActivatedEvent(Id, BusinessType));
    }

    /// <summary>Applies a data change from the Core Application. Queues Create if never submitted, otherwise Update.</summary>
    public void ApplyUpdate(decimal sumAssured, decimal premiumAmount, DateTime coverageEndDate)
    {
        if (Status is PolicyStatus.Terminated or PolicyStatus.Cancelled)
            throw new DomainException($"Cannot update a policy in status {Status}.");

        SumAssured = sumAssured;
        PremiumAmount = premiumAmount;
        CoverageEndDate = coverageEndDate;

        RecordHistory(Status, PolicyStatus.Updated, "Policy details updated");
        Status = PolicyStatus.Updated;
        AddDomainEvent(new PolicyUpdatedEvent(Id, BusinessType));
    }

    public void Renew(DateTime newCoverageEndDate, decimal? newPremiumAmount = null)
    {
        if (Status is PolicyStatus.Terminated or PolicyStatus.Cancelled)
            throw new DomainException($"Cannot renew a policy in status {Status}.");
        if (newCoverageEndDate <= CoverageEndDate)
            throw new DomainException("Renewal end date must extend beyond the current coverage end date.");

        CoverageEndDate = newCoverageEndDate;
        if (newPremiumAmount is not null) PremiumAmount = newPremiumAmount.Value;

        RecordHistory(Status, PolicyStatus.Renewed, "Policy renewed");
        Status = PolicyStatus.Renewed;
        AddDomainEvent(new PolicyRenewedEvent(Id, BusinessType, newCoverageEndDate));
    }

    public void Terminate(string reason)
    {
        if (Status is PolicyStatus.Terminated or PolicyStatus.Cancelled)
            throw new DomainException("Policy is already terminated/cancelled.");

        RecordHistory(Status, PolicyStatus.Terminated, $"Terminated: {reason}");
        Status = PolicyStatus.Terminated;
        AddDomainEvent(new PolicyCancelledEvent(Id, BusinessType, reason));
    }

    public void MarkDeletedFromNaicom()
    {
        RecordHistory(Status, PolicyStatus.Cancelled, "Deleted from NAICOM");
        Status = PolicyStatus.Cancelled;
        AddDomainEvent(new PolicyDeletedEvent(Id, BusinessType));
    }

    public void SetNaicomPolicyId(string naicomPolicyId)
    {
        NaicomPolicyId = naicomPolicyId;
    }

    public void MarkSubmissionResult(SubmissionStatus status, bool incrementRetry)
    {
        CurrentSubmissionStatus = status;
        LastSubmittedAt = DateTimeOffset.UtcNow;
        if (incrementRetry) RetryCount++;
        else if (status == SubmissionStatus.Completed) RetryCount = 0;
    }

    private void RecordHistory(PolicyStatus previous, PolicyStatus next, string notes)
        => _history.Add(new PolicyHistory(Id, previous, next, notes));
}
