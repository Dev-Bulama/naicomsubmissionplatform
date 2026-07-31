using Microsoft.EntityFrameworkCore;
using NLIP.Application.Common.Interfaces;
using NLIP.Domain.Entities.Common;
using NLIP.Infrastructure.Security;

namespace NLIP.Infrastructure.Services;

public class SettingsService : ISettingsService
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cache;
    private readonly SettingEncryptionService _encryption;

    public SettingsService(IApplicationDbContext context, ICacheService cache, SettingEncryptionService encryption)
    {
        _context = context;
        _cache = cache;
        _encryption = encryption;
    }

    public async Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync($"setting:{key}", async () =>
        {
            var setting = await _context.SystemSettings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == key, cancellationToken);
            if (setting?.Value is null) return (string?)null;
            return setting.IsSecret ? _encryption.Decrypt(setting.Value) : setting.Value;
        }, TimeSpan.FromMinutes(5), cancellationToken);
    }

    public async Task<string> GetRequiredAsync(string key, CancellationToken cancellationToken = default)
        => await GetAsync(key, cancellationToken) ?? throw new InvalidOperationException($"Required setting '{key}' is not configured.");

    public async Task<int> GetIntAsync(string key, int defaultValue, CancellationToken cancellationToken = default)
    {
        var value = await GetAsync(key, cancellationToken);
        return int.TryParse(value, out var parsed) ? parsed : defaultValue;
    }

    public async Task SetAsync(string key, string value, bool isSecret = false, CancellationToken cancellationToken = default)
    {
        var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == key, cancellationToken);
        var storedValue = isSecret ? _encryption.Encrypt(value) : value;

        if (setting is null)
        {
            _context.SystemSettings.Add(new SystemSetting { Key = key, Value = storedValue, IsSecret = isSecret });
        }
        else
        {
            setting.Value = storedValue;
            setting.IsSecret = isSecret;
            setting.ModifiedAt = DateTimeOffset.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync($"setting:{key}", cancellationToken);
    }
}
