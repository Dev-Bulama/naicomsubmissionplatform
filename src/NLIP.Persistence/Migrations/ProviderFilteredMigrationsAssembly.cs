using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;

namespace NLIP.Persistence.Migrations;

/// <summary>
/// NLIP.Persistence carries two independent migration sets in the SAME assembly — one per
/// supported database provider (see Migrations/SqlServer and Migrations/Postgres, and
/// docs/ROADMAP.md for why the Postgres set had to be hand-assembled rather than scaffolded the
/// normal way). Without this class, EF Core's default IMigrationsAssembly returns every
/// [Migration]-attributed class it finds in the assembly regardless of which provider is active,
/// so Database.MigrateAsync() would try to run SQL Server's T-SQL against Postgres (or vice
/// versa) and fail outright. This filters the migration list down to whichever provider's
/// sub-namespace (NLIP.Persistence.Migrations.SqlServer / ...Migrations.Postgres) matches
/// NlipDbContext's actual active provider name, so only the correct set is ever applied.
/// </summary>
#pragma warning disable EF1001 // MigrationsAssembly is an internal-support API; extending it is the documented pattern for this exact multi-provider scenario (see class summary).
public class ProviderFilteredMigrationsAssembly : MigrationsAssembly
{
    public ProviderFilteredMigrationsAssembly(
        ICurrentDbContext currentContext,
        IDbContextOptions options,
        IMigrationsIdGenerator idGenerator,
        IDiagnosticsLogger<DbLoggerCategory.Migrations> logger)
        : base(currentContext, options, idGenerator, logger)
    {
    }

    public override IReadOnlyDictionary<string, TypeInfo> Migrations
    {
        get
        {
            var suffix = ProviderNamespaceSuffix;
            return base.Migrations
                .Where(pair => pair.Value.Namespace is not null && pair.Value.Namespace.EndsWith(suffix, StringComparison.Ordinal))
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }

    /// <summary>Set from NlipDbContext's active provider — see DependencyInjection.AddPersistence,
    /// which knows whether it configured UseSqlServer or UseNpgsql at registration time.</summary>
    public static string ProviderNamespaceSuffix { get; set; } = "SqlServer";
}
#pragma warning restore EF1001
