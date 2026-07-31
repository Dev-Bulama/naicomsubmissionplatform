using NLIP.Domain.Entities.MasterData;

namespace NLIP.Persistence.Repositories;

public interface IUnitOfWork
{
    IRepository<Branch> Branches { get; }
    IRepository<Agent> Agents { get; }
    IRepository<Employer> Employers { get; }
    IRepository<Customer> Customers { get; }
    IRepository<Product> Products { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
