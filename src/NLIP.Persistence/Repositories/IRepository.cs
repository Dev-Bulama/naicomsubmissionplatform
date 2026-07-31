using System.Linq.Expressions;
using NLIP.Domain.Common;

namespace NLIP.Persistence.Repositories;

/// <summary>
/// Generic repository over master-data/reference entities (Branch, Agent, Employer, Customer,
/// Product, etc.). Policy itself is deliberately NOT accessed through this — its CQRS handlers
/// go through IApplicationDbContext directly because its queries need rich, query-specific
/// joins/projections (see GetPoliciesQuery) that a generic repository would only get in the way
/// of. This repository exists for the simpler, uniform CRUD screens (master-data management).
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);
}
