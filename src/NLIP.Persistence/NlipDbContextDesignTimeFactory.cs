using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using NLIP.Persistence.Migrations;

namespace NLIP.Persistence;

/// <summary>
/// Used only by `dotnet ef migrations ...` at design time — never constructed by the running
/// app (which configures NlipDbContext through DI in DependencyInjection.AddPersistence based on
/// Database:Provider). EF's tooling prefers an IDesignTimeDbContextFactory when one is present,
/// which is what lets this repo maintain two separate migration sets (SqlServer and Postgres —
/// see Migrations/SqlServer and Migrations/Postgres, and ProviderSpecificMigrationsAssembly which
/// makes sure only the active provider's migrations are ever applied at runtime).
///
/// Set NLIP_MIGRATION_PROVIDER=Postgres before running `dotnet ef migrations add ... --output-dir
/// Migrations/Postgres --namespace NLIP.Persistence.Migrations.Postgres`; omit it (or set
/// SqlServer) for the default SQL Server migration set. The connection string used here never
/// needs to be reachable — scaffolding a migration only needs to know the provider, not a live
/// database.
/// </summary>
public class NlipDbContextDesignTimeFactory : IDesignTimeDbContextFactory<NlipDbContext>
{
    public NlipDbContext CreateDbContext(string[] args)
    {
        var provider = Environment.GetEnvironmentVariable("NLIP_MIGRATION_PROVIDER") ?? "SqlServer";
        var optionsBuilder = new DbContextOptionsBuilder<NlipDbContext>();

        // Keep this in sync with the runtime filter — without it, `dotnet ef migrations script`/
        // `dbcontext script` see BOTH migration sets at once (confirmed: omitting this line makes
        // a SqlServer-targeted script scaffold also include the Postgres migration's operations).
        ProviderFilteredMigrationsAssembly.ProviderNamespaceSuffix = provider;

        if (string.Equals(provider, "Postgres", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseNpgsql(
                "Host=localhost;Database=nlip_design_time;Username=postgres;Password=postgres",
                npgsql => npgsql.MigrationsAssembly(typeof(NlipDbContext).Assembly.GetName().Name));
        }
        else
        {
            optionsBuilder.UseSqlServer(
                "Server=localhost;Database=NlipDb_DesignTime;Trusted_Connection=True;TrustServerCertificate=True",
                sql => sql.MigrationsAssembly(typeof(NlipDbContext).Assembly.GetName().Name));
        }

        optionsBuilder.ReplaceService<IMigrationsAssembly, ProviderFilteredMigrationsAssembly>();

        return new NlipDbContext(optionsBuilder.Options);
    }
}
