namespace NLIP.Application.Common.Interfaces;

/// <summary>Redis-backed cache abstraction (falls back to in-memory when Redis is not configured,
/// see NLIP.Infrastructure.Caching). Used for dashboard aggregates, settings, and NAICOM token caching.</summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
}
