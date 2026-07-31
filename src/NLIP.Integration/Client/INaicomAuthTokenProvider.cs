namespace NLIP.Integration.Client;

/// <summary>Obtains and caches the NAICOM bearer token, refreshing automatically shortly before
/// expiry. Internal to Integration — Application never sees tokens.</summary>
public interface INaicomAuthTokenProvider
{
    Task<string> GetTokenAsync(CancellationToken cancellationToken = default);
    Task InvalidateAsync(CancellationToken cancellationToken = default);
}
