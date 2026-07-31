using MediatR;

namespace NLIP.Domain.Common;

/// <summary>Root base for all domain entities. Carries a queue of domain events raised during
/// business operations; NLIP.Persistence dispatches and clears them inside SaveChangesAsync
/// (see DispatchDomainEventsInterceptor), which is also where Outbox rows are written in the
/// same DB transaction (Outbox pattern) so no event is ever lost on a crash between the two writes.</summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    private readonly List<INotification> _domainEvents = new();
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(INotification domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}

/// <summary>Base for entities that need full audit columns (Created/Modified by + timestamps).</summary>
public abstract class BaseAuditableEntity : BaseEntity
{
    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
}
