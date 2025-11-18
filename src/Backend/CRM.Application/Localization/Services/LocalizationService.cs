using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Localization.Services;

public class LocalizationService : ILocalizationService
{
    private readonly IAppDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly ILogger<LocalizationService> _logger;
    private const string CacheKeyPrefix = "localization_";
    private const int CacheExpirationMinutes = 60;

    public LocalizationService(IAppDbContext db, IMemoryCache cache, ILogger<LocalizationService> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<string> GetLocalizedStringAsync(string key, string languageCode)
    {
        var cacheKey = $"{CacheKeyPrefix}{languageCode}_{key}";
        
        if (_cache.TryGetValue(cacheKey, out string? cachedValue) && cachedValue != null)
        {
            return cachedValue;
        }

        var resource = await _db.LocalizationResources
            .FirstOrDefaultAsync(r => r.LanguageCode == languageCode &&
                                     r.ResourceKey == key &&
                                     r.IsActive);

        if (resource != null)
        {
            _cache.Set(cacheKey, resource.ResourceValue, TimeSpan.FromMinutes(CacheExpirationMinutes));
            return resource.ResourceValue;
        }

        // Fallback to English
        if (languageCode != "en")
        {
            return await GetFallbackStringAsync(key);
        }

        // If English also not found, return key
        _logger.LogWarning("Localization resource not found: {Key} for language: {Language}", key, languageCode);
        return key;
    }

    public async Task<Dictionary<string, string>> GetLocalizedStringsAsync(string languageCode, string? category = null)
    {
        try
        {
            var cacheKey = $"{CacheKeyPrefix}{languageCode}_{category ?? "all"}";
            
            if (_cache.TryGetValue(cacheKey, out Dictionary<string, string>? cachedDict) && cachedDict != null)
            {
                return cachedDict;
            }

            var query = _db.LocalizationResources
                .Where(r => r.LanguageCode == languageCode && r.IsActive);

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(r => r.Category == category);
            }

            var resources = await query.ToListAsync();
            var dictionary = resources.ToDictionary(r => r.ResourceKey, r => r.ResourceValue);

            _cache.Set(cacheKey, dictionary, TimeSpan.FromMinutes(CacheExpirationMinutes));
            return dictionary;
        }
        catch (Exception ex)
        {
            // Check if this is a missing table error (check both outer and inner exceptions)
            var exceptionMessage = ex.Message;
            var innerException = ex.InnerException;
            while (innerException != null)
            {
                exceptionMessage += " | " + innerException.Message;
                innerException = innerException.InnerException;
            }

            if (exceptionMessage.Contains("42P01") || 
                exceptionMessage.Contains("does not exist") || 
                (exceptionMessage.Contains("relation") && exceptionMessage.Contains("not exist")) ||
                exceptionMessage.Contains("Invalid object name") ||
                exceptionMessage.Contains("could not be found"))
            {
                _logger.LogWarning("LocalizationResources table does not exist, returning empty dictionary for language {LanguageCode}", languageCode);
                return new Dictionary<string, string>();
            }

            _logger.LogError(ex, "Error getting localized strings for language {LanguageCode}", languageCode);
            return new Dictionary<string, string>();
        }
    }

    public async Task<Dictionary<string, string>> GetAllResourcesForLanguageAsync(string languageCode)
    {
        return await GetLocalizedStringsAsync(languageCode);
    }

    public async Task<string> GetFallbackStringAsync(string key)
    {
        var resource = await _db.LocalizationResources
            .FirstOrDefaultAsync(r => r.LanguageCode == "en" &&
                                     r.ResourceKey == key &&
                                     r.IsActive);

        return resource?.ResourceValue ?? key;
    }
}


