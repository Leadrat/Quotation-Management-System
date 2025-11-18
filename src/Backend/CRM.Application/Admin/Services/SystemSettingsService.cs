using System.Text.Json;
using CRM.Application.Common.Persistence;
using CRM.Domain.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CRM.Application.Admin.Services;

public class SystemSettingsService : ISystemSettingsService
{
    private readonly IAppDbContext _db;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "system_settings_all";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

    public SystemSettingsService(IAppDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<Dictionary<string, object>> GetAllSettingsAsync()
    {
        if (_cache.TryGetValue<Dictionary<string, object>>(CacheKey, out var cachedSettings))
        {
            return cachedSettings!;
        }

        var settings = await _db.SystemSettings.ToListAsync();
        var result = new Dictionary<string, object>();

        foreach (var setting in settings)
        {
            result[setting.Key] = JsonSerializer.Deserialize<object>(setting.Value.RootElement.GetRawText()) ?? new object();
        }

        _cache.Set(CacheKey, result, CacheExpiration);
        return result;
    }

    public async Task<object?> GetSettingAsync(string key)
    {
        var allSettings = await GetAllSettingsAsync();
        return allSettings.TryGetValue(key, out var value) ? value : null;
    }

    public async Task UpdateSettingsAsync(Dictionary<string, object> settings, Guid modifiedBy)
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var kvp in settings)
        {
            var existing = await _db.SystemSettings.FindAsync(kvp.Key);

            if (existing != null)
            {
                // Update existing setting
                existing.Value = JsonDocument.Parse(JsonSerializer.Serialize(kvp.Value));
                existing.LastModifiedAt = now;
                existing.LastModifiedBy = modifiedBy;
            }
            else
            {
                // Create new setting
                var newSetting = new SystemSettings
                {
                    Key = kvp.Key,
                    Value = JsonDocument.Parse(JsonSerializer.Serialize(kvp.Value)),
                    LastModifiedAt = now,
                    LastModifiedBy = modifiedBy
                };
                _db.SystemSettings.Add(newSetting);
            }
        }

        await _db.SaveChangesAsync();
        InvalidateCache();
    }

    public void InvalidateCache()
    {
        _cache.Remove(CacheKey);
    }
}

