using MediatR;
using NLIP.Domain.Enums;

namespace NLIP.Domain.Events;

/// <summary>
/// Raised when a policy is first activated in the Core Insurance Application and must be
/// created on the NAICOM Portal. Handled by CreateNaicomSubmissionOnPolicyActivated, which
/// writes an outbox row (SubmissionQueue) in the same transaction as the entity change.
/// </summary>
public sealed record PolicyActivatedEvent(Guid PolicyId, BusinessType BusinessType) : INotification;

/// <summary>Raised when policy data changes after it already has a NAICOM policy ID (triggers Update endpoint).</summary>
public sealed record PolicyUpdatedEvent(Guid PolicyId, BusinessType BusinessType) : INotification;

/// <summary>Raised when a policy is renewed (triggers the Renew endpoint).</summary>
public sealed record PolicyRenewedEvent(Guid PolicyId, BusinessType BusinessType, DateTime NewCoverageEndDate) : INotification;

/// <summary>Raised when a policy is cancelled/terminated (triggers the Terminate endpoint).</summary>
public sealed record PolicyCancelledEvent(Guid PolicyId, BusinessType BusinessType, string Reason) : INotification;

/// <summary>Raised when a policy record is deleted locally and must also be removed from NAICOM, where permitted.</summary>
public sealed record PolicyDeletedEvent(Guid PolicyId, BusinessType BusinessType) : INotification;
