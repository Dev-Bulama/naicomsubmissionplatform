using Microsoft.EntityFrameworkCore;
using NLIP.Application.Common.Interfaces;
using NLIP.Domain.Entities.Identity;
using NLIP.Domain.Entities.MasterData;
using NLIP.Domain.Enums;
using NLIP.Shared.Constants;

namespace NLIP.Persistence.Seed;

/// <summary>
/// Idempotent startup seeding: system roles, the full permission catalog (Shared.PermissionNames),
/// a default SuperAdministrator role bound to every permission, one bootstrap admin user, and a
/// handful of master-data rows so a fresh environment can accept a policy submission immediately.
/// Called once from Program.cs behind an environment/config flag — never re-runs destructively.
/// </summary>
public static class DbInitializer
{
    public static async Task SeedAsync(NlipDbContext context, IPasswordHasher passwordHasher, CancellationToken cancellationToken = default)
    {
        await SeedPermissionsAsync(context, cancellationToken);
        await SeedRolesAsync(context, cancellationToken);
        await SeedAdminUserAsync(context, passwordHasher, cancellationToken);
        await SeedMasterDataAsync(context, cancellationToken);
        await SeedDefaultSettingsAsync(context, cancellationToken);
    }

    private static async Task SeedPermissionsAsync(NlipDbContext context, CancellationToken cancellationToken)
    {
        var existing = await context.Permissions.Select(p => p.Name).ToListAsync(cancellationToken);
        foreach (var name in PermissionNames.All.Except(existing))
            context.Permissions.Add(new Permission { Name = name, Description = name });

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedRolesAsync(NlipDbContext context, CancellationToken cancellationToken)
    {
        foreach (var roleName in RoleNames.All)
        {
            if (await context.Roles.AnyAsync(r => r.Name == roleName, cancellationToken)) continue;
            context.Roles.Add(new Role { Name = roleName, Description = roleName, IsSystemRole = true });
        }
        await context.SaveChangesAsync(cancellationToken);

        // SuperAdministrator gets every permission; other roles start with none assigned and are
        // configured by an admin via the Roles & Permissions screen (per "configurable permissions" requirement).
        var superAdmin = await context.Roles.FirstAsync(r => r.Name == RoleNames.SuperAdministrator, cancellationToken);
        var allPermissions = await context.Permissions.ToListAsync(cancellationToken);
        var assigned = await context.RolePermissions.Where(rp => rp.RoleId == superAdmin.Id).Select(rp => rp.PermissionId).ToListAsync(cancellationToken);

        foreach (var permission in allPermissions.Where(p => !assigned.Contains(p.Id)))
            context.RolePermissions.Add(new Domain.Entities.Identity.RolePermission { RoleId = superAdmin.Id, PermissionId = permission.Id });

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedAdminUserAsync(NlipDbContext context, IPasswordHasher passwordHasher, CancellationToken cancellationToken)
    {
        if (await context.Users.AnyAsync(cancellationToken)) return;

        var admin = new User
        {
            Username = "admin",
            Email = "admin@nlip.local",
            FullName = "NLIP System Administrator",
            PasswordHash = passwordHasher.Hash("ChangeMe!2026"),
            MustChangePassword = true,
            IsActive = true
        };
        context.Users.Add(admin);
        await context.SaveChangesAsync(cancellationToken);

        var superAdminRole = await context.Roles.FirstAsync(r => r.Name == RoleNames.SuperAdministrator, cancellationToken);
        context.UserRoles.Add(new Domain.Entities.Identity.UserRole { UserId = admin.Id, RoleId = superAdminRole.Id });
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedMasterDataAsync(NlipDbContext context, CancellationToken cancellationToken)
    {
        if (!await context.Branches.AnyAsync(cancellationToken))
        {
            context.Branches.Add(new Branch { Code = "HQ", Name = "Head Office", State = "Lagos" });
        }

        if (!await context.Products.AnyAsync(cancellationToken))
        {
            context.Products.Add(new Product { Code = "IND-LIFE-01", Name = "Individual Term Assurance", BusinessType = BusinessType.IndividualLife });
            context.Products.Add(new Product { Code = "GRP-LIFE-01", Name = "Group Life Assurance", BusinessType = BusinessType.GroupLife });
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedDefaultSettingsAsync(NlipDbContext context, CancellationToken cancellationToken)
    {
        var defaults = new (string Key, string Value, string Description, bool Secret)[]
        {
            (SettingKeys.NaicomBaseUrl, "https://portal.naicom.gov.ng/", "NAICOM Portal base URL", false),
            (SettingKeys.NaicomSid, string.Empty, "NAICOM subscriber ID", true),
            (SettingKeys.NaicomSecret, string.Empty, "NAICOM subscriber secret", true),
            (SettingKeys.NaicomTimeoutSeconds, "30", "Per-attempt HTTP timeout (seconds)", false),
            (SettingKeys.RetryMaxAttempts, "6", "Maximum retry attempts before dead-lettering a submission", false),
            (SettingKeys.RetrySchedule, "30,60,300,900,1800,3600", "Retry backoff schedule (seconds)", false),
            (SettingKeys.QueueMaxConcurrency, "10", "Max concurrent NAICOM submissions processed at once", false),
            (SettingKeys.LoggingMinimumLevel, "Information", "Serilog minimum level", false),
            (SettingKeys.SessionTimeoutMinutes, "30", "Idle session timeout (minutes)", false),
            (SettingKeys.AccountLockoutThreshold, "5", "Failed login attempts before lockout", false),
            (SettingKeys.AccountLockoutMinutes, "15", "Lockout duration (minutes)", false),
        };

        var existingKeys = await context.SystemSettings.Select(s => s.Key).ToListAsync(cancellationToken);
        foreach (var (key, value, description, secret) in defaults)
        {
            if (existingKeys.Contains(key)) continue;
            context.SystemSettings.Add(new Domain.Entities.Common.SystemSetting { Key = key, Value = value, Description = description, IsSecret = secret });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
