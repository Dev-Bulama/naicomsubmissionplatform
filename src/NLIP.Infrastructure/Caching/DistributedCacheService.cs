using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using NLIP.Application.Common.Interfaces;

namespace NLIP.Infrastructure.Caching;

/// <summary>
/// Wraps IDistributedCache (Redis in production via AddStackExchangeRedisCache, falling back to
/// AddDistributedMemoryCache when Redis:ConnectionString is not configured — see
/// DependencyInjection.AddInfrastructure) with JSON (de)serialization and a GetOrCreate helper.
/// </summary>
public class DistributedCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromMinutes(5);

    public DistributedCacheService(IDistributedCache cache) => _cache = cache;

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var bytes = await _cache.GetAsync(key, cancellationToken);
        return bytes is null ? default : JsonSerializer.Deserialize<T>(bytes);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
        await _cache.SetAsync(key, bytes, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiry ?? DefaultExpiry }, cancellationToken);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) => _cache.RemoveAsync(key, cancellationToken);

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached is not null) return cached;

        var value = await factory();
        await SetAsync(key, value, expiry, cancellationToken);
        return value;
    }
}
