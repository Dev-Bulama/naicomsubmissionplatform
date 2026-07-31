using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NLIP.Domain.Common;

namespace NLIP.Persistence.Interceptors;

/// <summary>
/// Dispatches every BaseEntity.DomainEvents via MediatR from inside SavingChangesAsync — i.e.
/// before EF Core executes the actual SQL for this SaveChanges call. Handlers such as
/// PolicySubmissionEventHandlers add new entities (SubmissionQueue rows) to this same context;
/// because EF Core only sends commands after SavingChangesAsync returns, those additions are
/// picked up by the same transaction as the change that raised the event — this is what makes
/// the Outbox pattern here actually transactional instead of "fire domain event, hope the
/// outbox write happens too."
/// </summary>
public class DispatchDomainEventsInterceptor : SaveChangesInterceptor
{
    private readonly IMediator _mediator;

    public DispatchDomainEventsInterceptor(IMediator mediator) => _mediator = mediator;

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(eventData.Context);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        DispatchDomainEventsAsync(eventData.Context).GetAwaiter().GetResult();
        return base.SavingChanges(eventData, result);
    }

    private async Task DispatchDomainEventsAsync(DbContext? context)
    {
        if (context is null) return;

        // Loop, not a single pass: a handler for one event may add an entity that itself carries
        // domain events (not used yet in this codebase, but keeps the interceptor correct if it
        // ever is).
        List<INotification> domainEvents;
        do
        {
            var entitiesWithEvents = context.ChangeTracker
                .Entries<BaseEntity>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Count > 0)
                .ToList();

            domainEvents = entitiesWithEvents.SelectMany(e => e.DomainEvents).ToList();
            entitiesWithEvents.ForEach(e => e.ClearDomainEvents());

            foreach (var domainEvent in domainEvents)
                await _mediator.Publish(domainEvent);
        }
        while (domainEvents.Count > 0);
    }
}
