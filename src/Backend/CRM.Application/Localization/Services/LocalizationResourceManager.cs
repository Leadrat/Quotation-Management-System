using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Localization.Dtos;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Localization.Services;

public class LocalizationResourceManager : ILocalizationResourceManager
{
    private readonly IAppDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly ILogger<LocalizationResourceManager> _logger;
    private const string CacheKeyPrefix = "localization_";

    public LocalizationResourceManager(IAppDbContext db, IMemoryCache cache, ILogger<LocalizationResourceManager> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<LocalizationResourceDto> CreateResourceAsync(CreateLocalizationResourceRequest request, Guid userId)
    {
        // Check if resource already exists
        var existing = await _db.LocalizationResources
            .FirstOrDefaultAsync(r => r.LanguageCode == request.LanguageCode &&
                                     r.ResourceKey == request.ResourceKey);

        if (existing != null)
        {
            throw new InvalidOperationException($"Resource with key '{request.ResourceKey}' already exists for language '{request.LanguageCode}'");
        }

        var resource = new LocalizationResource
        {
            ResourceId = Guid.NewGuid(),
            LanguageCode = request.LanguageCode,
            ResourceKey = request.ResourceKey,
            ResourceValue = request.ResourceValue,
            Category = request.Category,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            CreatedByUserId = userId
        };

        _db.LocalizationResources.Add(resource);
        await _db.SaveChangesAsync();

        // Invalidate cache
        InvalidateCache(request.LanguageCode);

        return new LocalizationResourceDto
        {
            ResourceId = resource.ResourceId,
            LanguageCode = resource.LanguageCode,
            ResourceKey = resource.ResourceKey,
            ResourceValue = resource.ResourceValue,
            Category = resource.Category,
            IsActive = resource.IsActive
        };
    }

    public async Task<LocalizationResourceDto> UpdateResourceAsync(Guid resourceId, UpdateLocalizationResourceRequest request, Guid userId)
    {
        var resource = await _db.LocalizationResources.FindAsync(resourceId);
        if (resource == null)
        {
            throw new InvalidOperationException("Resource not found");
        }

        if (request.ResourceValue != null)
            resource.ResourceValue = request.ResourceValue;
        if (request.Category != null)
            resource.Category = request.Category;
        if (request.IsActive.HasValue)
            resource.IsActive = request.IsActive.Value;

        resource.UpdatedAt = DateTimeOffset.UtcNow;
        resource.UpdatedByUserId = userId;

        await _db.SaveChangesAsync();

        // Invalidate cache
        InvalidateCache(resource.LanguageCode);

        return new LocalizationResourceDto
        {
            ResourceId = resource.ResourceId,
            LanguageCode = resource.LanguageCode,
            ResourceKey = resource.ResourceKey,
            ResourceValue = resource.ResourceValue,
            Category = resource.Category,
            IsActive = resource.IsActive
        };
    }

    public async Task DeleteResourceAsync(Guid resourceId)
    {
        var resource = await _db.LocalizationResources.FindAsync(resourceId);
        if (resource == null)
        {
            throw new InvalidOperationException("Resource not found");
        }

        _db.LocalizationResources.Remove(resource);
        await _db.SaveChangesAsync();

        // Invalidate cache
        InvalidateCache(resource.LanguageCode);
    }

    public async Task ImportResourcesAsync(string languageCode, Dictionary<string, string> resources, Guid userId)
    {
        var existingResources = await _db.LocalizationResources
            .Where(r => r.LanguageCode == languageCode)
            .ToListAsync();

        var existingKeys = existingResources.Select(r => r.ResourceKey).ToHashSet();

        foreach (var kvp in resources)
        {
            if (existingKeys.Contains(kvp.Key))
            {
                var existing = existingResources.First(r => r.ResourceKey == kvp.Key);
                existing.ResourceValue = kvp.Value;
                existing.UpdatedAt = DateTimeOffset.UtcNow;
                existing.UpdatedByUserId = userId;
            }
            else
            {
                var resource = new LocalizationResource
                {
                    ResourceId = Guid.NewGuid(),
                    LanguageCode = languageCode,
                    ResourceKey = kvp.Key,
                    ResourceValue = kvp.Value,
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    CreatedByUserId = userId
                };
                _db.LocalizationResources.Add(resource);
            }
        }

        await _db.SaveChangesAsync();

        // Invalidate cache
        InvalidateCache(languageCode);
    }

    public async Task<Dictionary<string, string>> ExportResourcesAsync(string languageCode)
    {
        var resources = await _db.LocalizationResources
            .Where(r => r.LanguageCode == languageCode && r.IsActive)
            .ToListAsync();

        return resources.ToDictionary(r => r.ResourceKey, r => r.ResourceValue);
    }

    private void InvalidateCache(string languageCode)
    {
        // Remove all cache entries for this language
        // Note: This is a simplified approach. In production, you might want to track cache keys more precisely.
        _cache.Remove($"{CacheKeyPrefix}{languageCode}_all");
    }
}


