using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NLIP.Domain.Common;

namespace NLIP.Persistence.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    private readonly NlipDbContext _context;
    private readonly DbSet<T> _set;

    public Repository(NlipDbContext context)
    {
        _context = context;
        _set = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _set.FindAsync(new object[] { id }, cancellationToken);

    public async Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _set.AsNoTracking();
        if (predicate is not null) query = query.Where(predicate);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default) => await _set.AddAsync(entity, cancellationToken);

    public void Update(T entity) => _context.Entry(entity).State = EntityState.Modified;

    public void Remove(T entity) => _set.Remove(entity);
}
