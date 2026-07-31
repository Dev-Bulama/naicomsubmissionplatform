using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLIP.Application.Common.Interfaces;
using NLIP.Persistence.Interceptors;
using NLIP.Persistence.Migrations;
using NLIP.Persistence.Repositories;
using NLIP.Shared.Configuration;

namespace NLIP.Persistence;

public static class DependencyInjection
{
    /// <param name="registerDbContext">
    /// False under the "Testing" environment (see NLIP.IntegrationTests.NlipWebApplicationFactory),
    /// which registers NlipDbContext itself pointed at an in-memory Sqlite connection instead.
    /// Registering SqlServer here first and then trying to swap it for Sqlite afterwards (via
    /// RemoveAll + re-Add in the test factory) does not work — EF Core ends up with both
    /// providers' services present and throws "Only a single database provider can be registered."
    /// Skipping the SqlServer registration entirely avoids that class of problem outright.
    /// </param>
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration, bool registerDbContext = true)
    {
        services.AddSingleton<DispatchDomainEventsInterceptor>();
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));

        var provider = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()?.Provider ?? DatabaseProvider.SqlServer;

        // Tells ProviderFilteredMigrationsAssembly which of the two migration sets (see
        // Migrations/SqlServer, Migrations/Postgres) actually applies — see that class for why
        // both exist in one assembly and why only one set may ever run against a given database.
        ProviderFilteredMigrationsAssembly.ProviderNamespaceSuffix = provider.ToString();

        if (registerDbContext)
        {
            services.AddDbContext<NlipDbContext>((sp, options) =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;

                if (provider == DatabaseProvider.Postgres)
                {
                    // Render (and most PaaS providers) hand out Postgres connection info as a
                    // postgres:// URI, not Npgsql's semicolon key=value format — see
                    // ConnectionStringNormalizer for why this conversion is needed.
                    options.UseNpgsql(
                        ConnectionStringNormalizer.NormalizePostgresUri(connectionString),
                        npgsql => npgsql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null));
                }
                else
                {
                    options.UseSqlServer(
                        connectionString,
                        sql => sql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null));
                }

                options.AddInterceptors(sp.GetRequiredService<DispatchDomainEventsInterceptor>());
                options.ReplaceService<IMigrationsAssembly, ProviderFilteredMigrationsAssembly>();
            });
        }

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<NlipDbContext>());
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        return services;
    }
}
