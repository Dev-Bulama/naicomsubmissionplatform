using Microsoft.Extensions.Logging;
using NLIP.Application.Common.Interfaces;
using NLIP.Domain.Enums;

namespace NLIP.Worker.Jobs;

/// <summary>Recurring API Health Monitor probe (every 2 minutes — see Program.cs). Broadcasts a
/// notification the moment NAICOM flips from available to unavailable (or back), rather than
/// spamming one per check, by comparing against the last cached state.</summary>
public class NaicomHealthCheckJob
{
    private const string LastStateCacheKey = "naicom:health:last-available";

    private readonly INaicomApiClient _naicomClient;
    private readonly ICacheService _cache;
    private readonly INotificationService _notifications;
    private readonly ILogger<NaicomHealthCheckJob> _logger;

    public NaicomHealthCheckJob(INaicomApiClient naicomClient, ICacheService cache, INotificationService notifications, ILogger<NaicomHealthCheckJob> logger)
    {
        _naicomClient = naicomClient;
        _cache = cache;
        _notifications = notifications;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var status = await _naicomClient.CheckHealthAsync(cancellationToken);
        _logger.LogInformation("NLIP NAICOM health: available={Available} version={Version} responseTime={ResponseMs}ms",
            status.IsAvailable, status.Version, status.ResponseTimeMs);

        var lastAvailable = await _cache.GetAsync<bool?>(LastStateCacheKey, cancellationToken);
        if (lastAvailable is not null && lastAvailable != status.IsAvailable)
        {
            await _notifications.BroadcastAsync(
                status.IsAvailable ? "NAICOM Portal Available" : "NAICOM Portal Unavailable",
                status.IsAvailable ? "NAICOM API connectivity has been restored." : $"NAICOM API is unreachable: {status.ErrorMessage}",
                NotificationChannel.SignalR, cancellationToken: cancellationToken);
        }

        await _cache.SetAsync(LastStateCacheKey, status.IsAvailable, TimeSpan.FromHours(1), cancellationToken);
    }
}
