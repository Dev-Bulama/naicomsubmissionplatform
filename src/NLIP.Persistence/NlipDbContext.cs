using Microsoft.EntityFrameworkCore;
using NLIP.Application.Common.Interfaces;
using NLIP.Domain.Entities.Common;
using NLIP.Domain.Entities.Identity;
using NLIP.Domain.Entities.Integration;
using NLIP.Domain.Entities.MasterData;
using NLIP.Domain.Entities.Policies;

namespace NLIP.Persistence;

public class NlipDbContext : DbContext, IApplicationDbContext
{
    public NlipDbContext(DbContextOptions<NlipDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<Employer> Employers => Set<Employer>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();

    public DbSet<Policy> Policies => Set<Policy>();
    public DbSet<PolicyBeneficiary> PolicyBeneficiaries => Set<PolicyBeneficiary>();
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
    public DbSet<PolicyHistory> PolicyHistories => Set<PolicyHistory>();

    public DbSet<NaicomTransaction> NaicomTransactions => Set<NaicomTransaction>();
    public DbSet<SubmissionQueue> SubmissionQueue => Set<SubmissionQueue>();
    public DbSet<ApiCallLog> ApiCallLogs => Set<ApiCallLog>();
    public DbSet<BackgroundJobRecord> BackgroundJobRecords => Set<BackgroundJobRecord>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<ErrorLog> ErrorLogs => Set<ErrorLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // BaseEntity.DomainEvents is transient plumbing (see DispatchDomainEventsInterceptor),
        // never a column/navigation — ignore it on every entity so EF's convention-based model
        // discovery doesn't try to map it.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(Domain.Common.BaseEntity).IsAssignableFrom(entityType.ClrType))
                modelBuilder.Entity(entityType.ClrType).Ignore(nameof(Domain.Common.BaseEntity.DomainEvents));
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NlipDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
        base.ConfigureConventions(configurationBuilder);
    }
}
