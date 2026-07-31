using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLIP.Application.Common.Interfaces;
using NLIP.Integration.Dtos.Auth;

namespace NLIP.Integration.Client;

/// <summary>
/// Exchanges the configured SID/Secret for a bearer token and caches it (via ICacheService —
/// Redis in production, see NLIP.Infrastructure.Caching) for slightly less than its stated
/// lifetime, so concurrent requests share one token instead of each re-authenticating.
/// </summary>
public class NaicomAuthTokenProvider : INaicomAuthTokenProvider
{
    private const string CacheKey = "naicom:access-token";

    private readonly HttpClient _httpClient;
    private readonly ICacheService _cache;
    private readonly ISettingsService _settings;
    private readonly ILogger<NaicomAuthTokenProvider> _logger;
    private readonly NaicomOptions _options;

    public NaicomAuthTokenProvider(
        HttpClient httpClient, ICacheService cache, ISettingsService settings,
        IOptions<NaicomOptions> options, ILogger<NaicomAuthTokenProvider> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _settings = settings;
        _options = options.Value;
        _logger = logger;
    }

    public Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
        => _cache.GetOrCreateAsync(CacheKey, () => AuthenticateAsync(cancellationToken), TimeSpan.FromMinutes(50), cancellationToken);

    public Task InvalidateAsync(CancellationToken cancellationToken = default) => _cache.RemoveAsync(CacheKey, cancellationToken);

    private async Task<string> AuthenticateAsync(CancellationToken cancellationToken)
    {
        var sid = await _settings.GetAsync(Shared.Constants.SettingKeys.NaicomSid, cancellationToken) ?? _options.Sid;
        var secret = await _settings.GetAsync(Shared.Constants.SettingKeys.NaicomSecret, cancellationToken) ?? _options.Secret;

        _logger.LogInformation("NLIP NAICOM: requesting new access token");

        var response = await _httpClient.PostAsJsonAsync(NaicomApiEndpoints.Authenticate,
            new NaicomTokenRequestDto { Sid = sid, Secret = secret }, cancellationToken);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<NaicomTokenResponseDto>(cancellationToken: cancellationToken)
            ?? throw new Application.Common.Exceptions.NaicomApiException("NAICOM auth endpoint returned an empty response.");

        return payload.AccessToken;
    }
}
