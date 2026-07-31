using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NLIP.Persistence;

namespace NLIP.IntegrationTests;

/// <summary>
/// Boots the real NLIP.API host under the "Testing" environment (see Program.cs — this skips the
/// SQL Server migrate/seed step and, critically, skips registering NlipDbContext against
/// SqlServer at all: AddPersistence(..., registerDbContext: false)). This factory then registers
/// NlipDbContext itself against an in-memory Sqlite connection, built straight from the EF model
/// via EnsureCreated() since no EF migration files exist yet in this scaffold (see
/// docs/ROADMAP.md). Registering Sqlite on top of an already-registered SqlServer DbContext
/// (e.g. via RemoveAll + re-Add) does NOT work — EF Core ends up with both providers' services
/// present in the container and throws "Only a single database provider can be registered";
/// skipping the SqlServer registration in the first place avoids that.
/// </summary>
public class NlipWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
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
