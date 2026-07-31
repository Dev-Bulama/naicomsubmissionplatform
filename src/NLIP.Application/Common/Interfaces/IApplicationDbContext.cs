using Microsoft.EntityFrameworkCore;
using NLIP.Domain.Entities.Common;
using NLIP.Domain.Entities.Identity;
using NLIP.Domain.Entities.Integration;
using NLIP.Domain.Entities.MasterData;
using NLIP.Domain.Entities.Policies;

namespace NLIP.Application.Common.Interfaces;

/// <summary>
/// Persistence-agnostic view of the database used by Application-layer handlers. Implemented by
/// NLIP.Persistence.NlipDbContext so Application never takes a direct dependency on EF Core's
/// concrete DbContext or on SQL Server.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<Branch> Branches { get; }
    DbSet<Agent> Agents { get; }
    DbSet<Employer> Employers { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Product> Products { get; }

    DbSet<Policy> Policies { get; }
    DbSet<PolicyBeneficiary> PolicyBeneficiaries { get; }
    DbSet<GroupMember> GroupMembers { get; }
    DbSet<PolicyHistory> PolicyHistories { get; }

    DbSet<NaicomTransaction> NaicomTransactions { get; }
    DbSet<SubmissionQueue> SubmissionQueue { get; }
    DbSet<ApiCallLog> ApiCallLogs { get; }
    DbSet<BackgroundJobRecord> BackgroundJobRecords { get; }

    DbSet<AuditLog> AuditLogs { get; }
    DbSet<ActivityLog> ActivityLogs { get; }
    DbSet<ErrorLog> ErrorLogs { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<SystemSetting> SystemSettings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
