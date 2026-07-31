using Microsoft.Extensions.Configuration;
using NLIP.Shared.Configuration;
using Serilog;
using Serilog.Events;

namespace NLIP.Infrastructure.Logging;

/// <summary>
/// Shared Serilog bootstrap for every host (API, Web, Worker) so log shape/enrichment is
/// identical across processes — required for correlating a request across API -> outbox ->
/// Worker -> NAICOM in a single Correlation ID. Sinks: Console (always), rolling file, and — for
/// SQL Server deployments only — a SQL sink (same database as the app, "SerilogLogs" table).
/// ElasticSearch-ready via an optional Serilog.Sinks.Elasticsearch package addition without
/// touching call sites.
/// </summary>
public static class SerilogConfigurator
{
    /// <param name="enableDatabaseSink">
    /// Pass false under the "Testing" environment (see NLIP.IntegrationTests) — the MSSqlServer
    /// sink connects and creates its table eagerly at construction time, which would otherwise
    /// crash every integration test before a single request is handled, since the test host has
    /// no real SQL Server behind it.
    /// </param>
    public static LoggerConfiguration Configure(IConfiguration configuration, string applicationName, bool enableDatabaseSink = true)
    {
        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Is(ParseLevel(configuration["Logging:MinimumLevel"]))
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithCorrelationId()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("Application", applicationName)
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Application} {Message:lj}{NewLine}{Exception}")
            .WriteTo.File("logs/nlip-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30);

        // The SQL sink only understands SQL Server's wire protocol — under the Postgres deployment
        // path (Database:Provider=Postgres, see render.yaml/docs/DEPLOYMENT.md) this is skipped
        // entirely and logging stays console/file-only rather than pointing MSSqlServer at a
        // Postgres connection string, which would fail every write. Add
        // Serilog.Sinks.PostgreSQL here if a Postgres DB log sink becomes a requirement.
        var provider = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()?.Provider ?? DatabaseProvider.SqlServer;
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (enableDatabaseSink && provider == DatabaseProvider.SqlServer && !string.IsNullOrWhiteSpace(connectionString))
        {
            try
            {
                loggerConfig = loggerConfig.WriteTo.MSSqlServer(
                    connectionString: connectionString,
                    sinkOptions: new Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions { TableName = "SerilogLogs", AutoCreateSqlTable = true });
            }
            catch (Exception ex)
            {
                // The MSSqlServer sink connects and creates its table synchronously right here —
                // a transiently unreachable DB at boot must not crash the whole app; fall back to
                // console/file logging only and let health checks/monitoring surface the DB issue.
                Console.Error.WriteLine($"NLIP: could not initialize the SQL Server log sink, continuing with console/file only: {ex.Message}");
            }
        }

        return loggerConfig;
    }

    private static LogEventLevel ParseLevel(string? value) =>
        Enum.TryParse<LogEventLevel>(value, true, out var level) ? level : LogEventLevel.Information;
}
