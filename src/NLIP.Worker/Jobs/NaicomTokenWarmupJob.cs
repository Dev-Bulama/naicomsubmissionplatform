using Microsoft.Extensions.Logging;
using NLIP.Integration.Client;

namespace NLIP.Worker.Jobs;

/// <summary>Proactively refreshes the cached NAICOM bearer token a few minutes before it would
/// expire (the token cache TTL in NaicomAuthTokenProvider is 50 minutes against a typical 60
/// minute token lifetime), so a real submission never pays the extra auth round-trip latency.</summary>
public class NaicomTokenWarmupJob
{
    private readonly INaicomAuthTokenProvider _tokenProvider;
    private readonly ILogger<NaicomTokenWarmupJob> _logger;

    public NaicomTokenWarmupJob(INaicomAuthTokenProvider tokenProvider, ILogger<NaicomTokenWarmupJob> logger)
    {
        _tokenProvider = tokenProvider;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _tokenProvider.InvalidateAsync(cancellationToken);
            await _tokenProvider.GetTokenAsync(cancellationToken);
            _logger.LogInformation("NLIP NAICOM: token warmup succeeded");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "NLIP NAICOM: token warmup failed — next real submission will retry authentication");
        }
    }
}
