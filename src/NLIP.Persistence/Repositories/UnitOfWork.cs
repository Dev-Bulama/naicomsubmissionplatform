using NLIP.Domain.Entities.MasterData;

namespace NLIP.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly NlipDbContext _context;

    public UnitOfWork(NlipDbContext context)
    {
        _context = context;
        Branches = new Repository<Branch>(context);
        Agents = new Repository<Agent>(context);
        Employers = new Repository<Employer>(context);
        Customers = new Repository<Customer>(context);
        Products = new Repository<Product>(context);
    }

    public IRepository<Branch> Branches { get; }
    public IRepository<Agent> Agents { get; }
    public IRepository<Employer> Employers { get; }
    public IRepository<Customer> Customers { get; }
    public IRepository<Product> Products { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);
}
