using CRM.Application.Admin.DTOs;

namespace CRM.Application.Admin.Services;

/// <summary>
/// Service for managing integration keys with encryption
/// </summary>
public interface IIntegrationKeyService
{
    /// <summary>
    /// Gets all integration keys (without decrypted values)
    /// </summary>
    Task<List<IntegrationKeyDto>> GetAllKeysAsync();

    /// <summary>
    /// Gets a specific integration key by ID (without decrypted value)
    /// </summary>
    Task<IntegrationKeyDto?> GetKeyByIdAsync(Guid id);

    /// <summary>
    /// Gets a decrypted key value (for display only, temporary)
    /// </summary>
    Task<IntegrationKeyWithValueDto?> GetKeyWithValueAsync(Guid id);

    /// <summary>
    /// Creates a new integration key (encrypts the value)
    /// </summary>
    Task<IntegrationKeyDto> CreateKeyAsync(string keyName, string keyValue, string provider, Guid createdBy);

    /// <summary>
    /// Updates an integration key
    /// </summary>
    Task<IntegrationKeyDto> UpdateKeyAsync(Guid id, string? keyName, string? keyValue, string? provider, Guid updatedBy);

    /// <summary>
    /// Deletes an integration key
    /// </summary>
    Task<bool> DeleteKeyAsync(Guid id);

    /// <summary>
    /// Marks a key as used (updates LastUsedAt)
    /// </summary>
    Task MarkKeyAsUsedAsync(Guid id);
}

