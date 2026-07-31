namespace NLIP.Application.Common.Interfaces;

/// <summary>Typed accessor over the SystemSettings table (see NLIP.Shared.Constants.SettingKeys).
/// Cached with a short TTL via ICacheService so hot paths (e.g. building the NAICOM HttpClient
/// per request) don't hit the database.</summary>
public interface ISettingsService
{
    Task<string?> GetAsync(string key, CancellationToken cancellationToken = default);
    Task<string> GetRequiredAsync(string key, CancellationToken cancellationToken = default);
    Task<int> GetIntAsync(string key, int defaultValue, CancellationToken cancellationToken = default);
    Task SetAsync(string key, string value, bool isSecret = false, CancellationToken cancellationToken = default);
}
