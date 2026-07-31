using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NLIP.Application.Common.Interfaces;
using NLIP.Domain.Entities.Integration;
using NLIP.Domain.Enums;
using NLIP.Domain.Events;

namespace NLIP.Application.Features.Policies.EventHandlers;

/// <summary>
/// Turns each policy domain event into an outbox row (SubmissionQueue). These handlers run
/// synchronously inside DispatchDomainEventsInterceptor.SavingChangesAsync — i.e. before the
/// owning SaveChangesAsync call commits — so the SubmissionQueue insert lands in the exact same
/// database transaction as the Policy status change that triggered it (classic Outbox pattern:
/// either both are committed or neither is). Nothing here calls NAICOM directly; that happens
/// later, out-of-band, in the Hangfire outbox processor (NLIP.Worker).
/// </summary>
public class PolicySubmissionEventHandlers :
    INotificationHandler<PolicyActivatedEvent>,
    INotificationHandler<PolicyUpdatedEvent>,
    INotificationHandler<PolicyRenewedEvent>,
    INotificationHandler<PolicyCancelledEvent>,
    INotificationHandler<PolicyDeletedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<PolicySubmissionEventHandlers> _logger;

    public PolicySubmissionEventHandlers(IApplicationDbContext context, ILogger<PolicySubmissionEventHandlers> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task Handle(PolicyActivatedEvent notification, CancellationToken cancellationToken)
        => EnqueueAsync(notification.PolicyId, NaicomAction.Create, cancellationToken);

    public Task Handle(PolicyUpdatedEvent notification, CancellationToken cancellationToken)
        => EnqueueAsync(notification.PolicyId, NaicomAction.Update, cancellationToken);

    public Task Handle(PolicyRenewedEvent notification, CancellationToken cancellationToken)
        => EnqueueAsync(notification.PolicyId, NaicomAction.Renew, cancellationToken);

    public Task Handle(PolicyCancelledEvent notification, CancellationToken cancellationToken)
        => EnqueueAsync(notification.PolicyId, NaicomAction.Terminate, cancellationToken);

    public Task Handle(PolicyDeletedEvent notification, CancellationToken cancellationToken)
        => EnqueueAsync(notification.PolicyId, NaicomAction.Delete, cancellationToken);

    private async Task EnqueueAsync(Guid policyId, NaicomAction action, CancellationToken cancellationToken)
    {
        var policy = await _context.Policies
            .Include(p => p.Beneficiaries)
            .Include(p => p.GroupMembers)
            .FirstOrDefaultAsync(p => p.Id == policyId, cancellationToken);

        if (policy is null)
        {
            _logger.LogWarning("NLIP Outbox: policy {PolicyId} not found when enqueuing {Action}", policyId, action);
            return;
        }

        // Determine the real action: if a Create was requested but the policy already has a
        // NAICOM ID (e.g. re-activation after a manual fix), submit an Update instead.
        var effectiveAction = action == NaicomAction.Create && policy.NaicomPolicyId is not null
            ? NaicomAction.Update
            : action;

        var snapshot = new
        {
            policy.Id,
            policy.PolicyNumber,
            policy.CorePolicyId,
            policy.NaicomPolicyId,
            policy.BusinessType,
            policy.SumAssured,
            policy.PremiumAmount,
            policy.CoverageStartDate,
            policy.CoverageEndDate,
            Beneficiaries = policy.Beneficiaries.Select(b => new { b.FullName, b.Relationship, b.DateOfBirth, b.SharePercentage }),
            GroupMembers = policy.GroupMembers.Select(m => new { m.EmployeeId, m.FullName, m.DateOfBirth, m.Gender, m.SumAssured })
        };

        _context.SubmissionQueue.Add(new SubmissionQueue
        {
            PolicyId = policy.Id,
            Action = effectiveAction,
            Payload = JsonSerializer.Serialize(snapshot),
            Status = SubmissionStatus.Queued,
            NextAttemptAt = DateTimeOffset.UtcNow
        });
    }
}
