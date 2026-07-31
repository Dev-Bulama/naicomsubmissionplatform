using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace NLIP.Infrastructure.Logging;

/// <summary>
/// Shared Serilog bootstrap for every host (API, Web, Worker) so log shape/enrichment is
/// identical across processes — required for correlating a request across API -> outbox ->
/// Worker -> NAICOM in a single Correlation ID. Sinks: Console (always), rolling file, and SQL
/// Server (same database as the app, ErrorLog-adjacent "Logs" table) — ElasticSearch-ready via
/// an optional Serilog.Sinks.Elasticsearch package addition without touching call sites.
/// </summary>
public static class SerilogConfigurator
{
    public static LoggerConfiguration Configure(IConfiguration configuration, string applicationName)
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

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            loggerConfig = loggerConfig.WriteTo.MSSqlServer(
                connectionString: connectionString,
                sinkOptions: new Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions { TableName = "SerilogLogs", AutoCreateSqlTable = true });
        }

        return loggerConfig;
    }

    private static LogEventLevel ParseLevel(string? value) =>
        Enum.TryParse<LogEventLevel>(value, true, out var level) ? level : LogEventLevel.Information;
}
