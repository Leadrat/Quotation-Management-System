using CRM.Application.Admin.DTOs;

namespace CRM.Application.Admin.Services;

/// <summary>
/// Service for managing data retention policies
/// </summary>
public interface IDataRetentionService
{
    /// <summary>
    /// Gets all retention policies
    /// </summary>
    Task<List<DataRetentionPolicyDto>> GetAllPoliciesAsync();

    /// <summary>
    /// Gets a policy by entity type
    /// </summary>
    Task<DataRetentionPolicyDto?> GetPolicyByEntityTypeAsync(string entityType);

    /// <summary>
    /// Creates or updates a retention policy
    /// </summary>
    Task<DataRetentionPolicyDto> UpsertPolicyAsync(
        string entityType,
        int retentionPeriodMonths,
        bool isActive,
        bool autoPurgeEnabled,
        Guid updatedBy);
}

