using Hangfire;
using NLIP.Application;
using NLIP.Infrastructure;
using NLIP.Infrastructure.Logging;
using NLIP.Integration;
using NLIP.Persistence;
using NLIP.Worker.Jobs;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = SerilogConfigurator.Configure(builder.Configuration, "NLIP.Worker").CreateLogger();
builder.Logging.ClearProviders();
builder.Services.AddSerilog();

builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddIntegration(builder.Configuration);

// This process is the only one that actually executes Hangfire jobs (AddHangfireServer); the
// API/Web hosts share the same storage (registered by AddInfrastructure -> AddHangfireClient)
// purely as enqueue-only clients.
builder.Services.AddHangfireServer(options =>
{
    options.Queues = new[] { "default", "submissions", "recurring" };
    options.WorkerCount = Math.Max(Environment.ProcessorCount, 2);
});

builder.Services.AddScoped<OutboxProcessorJob>();
builder.Services.AddScoped<NaicomHealthCheckJob>();
builder.Services.AddScoped<NaicomTokenWarmupJob>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var recurring = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    // Outbox drain: fast cadence so a queued submission doesn't sit long before being picked up.
    recurring.AddOrUpdate<OutboxProcessorJob>("outbox-processor", job => job.RunAsync(CancellationToken.None), "*/15 * * * * *");

    // API Health Monitor probe.
    recurring.AddOrUpdate<NaicomHealthCheckJob>("naicom-health-check", job => job.RunAsync(CancellationToken.None), "*/2 * * * *");

    // Token refresh well ahead of the ~50 minute cache TTL.
    recurring.AddOrUpdate<NaicomTokenWarmupJob>("naicom-token-warmup", job => job.RunAsync(CancellationToken.None), "*/45 * * * *");
}

Log.Information("NLIP Worker started");
host.Run();
