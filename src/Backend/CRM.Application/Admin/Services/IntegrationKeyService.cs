using CRM.Application.Admin.DTOs;
using CRM.Application.Common.Persistence;
using CRM.Application.Common.Services;
using CRM.Domain.Admin;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Admin.Services;

public class IntegrationKeyService : IIntegrationKeyService
{
    private readonly IAppDbContext _db;
    private readonly CRM.Application.Common.Services.IDataEncryptionService _encryptionService;

    public IntegrationKeyService(IAppDbContext db, CRM.Application.Common.Services.IDataEncryptionService encryptionService)
    {
        _db = db;
        _encryptionService = encryptionService;
    }

    public async Task<List<IntegrationKeyDto>> GetAllKeysAsync()
    {
        var keys = await _db.IntegrationKeys
            .OrderBy(k => k.Provider)
            .ThenBy(k => k.KeyName)
            .ToListAsync();

        return keys.Select(k => new IntegrationKeyDto
        {
            Id = k.Id,
            KeyName = k.KeyName,
            Provider = k.Provider,
            CreatedAt = k.CreatedAt,
            UpdatedAt = k.UpdatedAt,
            LastUsedAt = k.LastUsedAt,
            CreatedBy = k.CreatedBy,
            UpdatedBy = k.UpdatedBy
        }).ToList();
    }

    public async Task<IntegrationKeyDto?> GetKeyByIdAsync(Guid id)
    {
        var key = await _db.IntegrationKeys.FindAsync(id);
        if (key == null) return null;

        return new IntegrationKeyDto
        {
            Id = key.Id,
            KeyName = key.KeyName,
            Provider = key.Provider,
            CreatedAt = key.CreatedAt,
            UpdatedAt = key.UpdatedAt,
            LastUsedAt = key.LastUsedAt,
            CreatedBy = key.CreatedBy,
            UpdatedBy = key.UpdatedBy
        };
    }

    public async Task<IntegrationKeyWithValueDto?> GetKeyWithValueAsync(Guid id)
    {
        var key = await _db.IntegrationKeys.FindAsync(id);
        if (key == null) return null;

        try
        {
            var decryptedValue = _encryptionService.Decrypt(key.KeyValueEncrypted);

            return new IntegrationKeyWithValueDto
            {
                Id = key.Id,
                KeyName = key.KeyName,
                Provider = key.Provider,
                CreatedAt = key.CreatedAt,
                UpdatedAt = key.UpdatedAt,
                LastUsedAt = key.LastUsedAt,
                CreatedBy = key.CreatedBy,
                UpdatedBy = key.UpdatedBy,
                KeyValue = decryptedValue
            };
        }
        catch
        {
            // If decryption fails, return null
            return null;
        }
    }

    public async Task<IntegrationKeyDto> CreateKeyAsync(string keyName, string keyValue, string provider, Guid createdBy)
    {
        var now = DateTimeOffset.UtcNow;
        var encryptedValue = _encryptionService.Encrypt(keyValue);

        var key = new IntegrationKey
        {
            Id = Guid.NewGuid(),
            KeyName = keyName,
            KeyValueEncrypted = encryptedValue,
            Provider = provider,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = createdBy
        };

        _db.IntegrationKeys.Add(key);
        await _db.SaveChangesAsync();

        return new IntegrationKeyDto
        {
            Id = key.Id,
            KeyName = key.KeyName,
            Provider = key.Provider,
            CreatedAt = key.CreatedAt,
            UpdatedAt = key.UpdatedAt,
            LastUsedAt = key.LastUsedAt,
            CreatedBy = key.CreatedBy,
            UpdatedBy = key.UpdatedBy
        };
    }

    public async Task<IntegrationKeyDto> UpdateKeyAsync(Guid id, string? keyName, string? keyValue, string? provider, Guid updatedBy)
    {
        var key = await _db.IntegrationKeys.FindAsync(id);
        if (key == null)
        {
            throw new KeyNotFoundException($"Integration key with ID {id} not found");
        }

        if (!string.IsNullOrEmpty(keyName))
        {
            key.KeyName = keyName;
        }

        if (!string.IsNullOrEmpty(keyValue))
        {
            key.KeyValueEncrypted = _encryptionService.Encrypt(keyValue);
        }

        if (!string.IsNullOrEmpty(provider))
        {
            key.Provider = provider;
        }

        key.UpdatedAt = DateTimeOffset.UtcNow;
        key.UpdatedBy = updatedBy;

        await _db.SaveChangesAsync();

        return new IntegrationKeyDto
        {
            Id = key.Id,
            KeyName = key.KeyName,
            Provider = key.Provider,
            CreatedAt = key.CreatedAt,
            UpdatedAt = key.UpdatedAt,
            LastUsedAt = key.LastUsedAt,
            CreatedBy = key.CreatedBy,
            UpdatedBy = key.UpdatedBy
        };
    }

    public async Task<bool> DeleteKeyAsync(Guid id)
    {
        var key = await _db.IntegrationKeys.FindAsync(id);
        if (key == null) return false;

        _db.IntegrationKeys.Remove(key);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task MarkKeyAsUsedAsync(Guid id)
    {
        var key = await _db.IntegrationKeys.FindAsync(id);
        if (key != null)
        {
            key.LastUsedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}

