using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NLIP.Persistence;

namespace NLIP.IntegrationTests;

/// <summary>
/// Boots the real NLIP.API host under the "Testing" environment (see Program.cs — this skips
/// the SQL Server migrate/seed step) and swaps NlipDbContext onto an in-memory Sqlite connection
/// built straight from the EF model via EnsureCreated(), since no EF migration files exist yet
/// in this scaffold (see docs/ROADMAP.md). Keeps the connection open for the factory's lifetime
/// so the in-memory database isn't dropped between requests within a test.
/// </summary>
public class NlipWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<NlipDbContext>>();

            _connection.Open();
            services.AddDbContext<NlipDbContext>(options => options.UseSqlite(_connection));

            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<NlipDbContext>();
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection.Dispose();
    }
}
