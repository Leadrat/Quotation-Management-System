using CRM.Application.Admin.DTOs;
using CRM.Application.Common.Persistence;
using CRM.Domain.Admin;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Admin.Services;

public class DataRetentionService : IDataRetentionService
{
    private readonly IAppDbContext _db;

    public DataRetentionService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<DataRetentionPolicyDto>> GetAllPoliciesAsync()
    {
        var policies = await _db.DataRetentionPolicies
            .OrderBy(p => p.EntityType)
            .ToListAsync();

        return policies.Select(p => new DataRetentionPolicyDto
        {
            Id = p.Id,
            EntityType = p.EntityType,
            RetentionPeriodMonths = p.RetentionPeriodMonths,
            IsActive = p.IsActive,
            AutoPurgeEnabled = p.AutoPurgeEnabled,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            CreatedBy = p.CreatedBy,
            UpdatedBy = p.UpdatedBy
        }).ToList();
    }

    public async Task<DataRetentionPolicyDto?> GetPolicyByEntityTypeAsync(string entityType)
    {
        var policy = await _db.DataRetentionPolicies
            .FirstOrDefaultAsync(p => p.EntityType == entityType);

        if (policy == null) return null;

        return new DataRetentionPolicyDto
        {
            Id = policy.Id,
            EntityType = policy.EntityType,
            RetentionPeriodMonths = policy.RetentionPeriodMonths,
            IsActive = policy.IsActive,
            AutoPurgeEnabled = policy.AutoPurgeEnabled,
            CreatedAt = policy.CreatedAt,
            UpdatedAt = policy.UpdatedAt,
            CreatedBy = policy.CreatedBy,
            UpdatedBy = policy.UpdatedBy
        };
    }

    public async Task<DataRetentionPolicyDto> UpsertPolicyAsync(
        string entityType,
        int retentionPeriodMonths,
        bool isActive,
        bool autoPurgeEnabled,
        Guid updatedBy)
    {
        var policy = await _db.DataRetentionPolicies
            .FirstOrDefaultAsync(p => p.EntityType == entityType);

        var now = DateTimeOffset.UtcNow;

        if (policy == null)
        {
            // Create new policy
            policy = new DataRetentionPolicy
            {
                Id = Guid.NewGuid(),
                EntityType = entityType,
                RetentionPeriodMonths = retentionPeriodMonths,
                IsActive = isActive,
                AutoPurgeEnabled = autoPurgeEnabled,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = updatedBy
            };
            _db.DataRetentionPolicies.Add(policy);
        }
        else
        {
            // Update existing policy
            policy.RetentionPeriodMonths = retentionPeriodMonths;
            policy.IsActive = isActive;
            policy.AutoPurgeEnabled = autoPurgeEnabled;
            policy.UpdatedAt = now;
            policy.UpdatedBy = updatedBy;
        }

        await _db.SaveChangesAsync();

        return new DataRetentionPolicyDto
        {
            Id = policy.Id,
            EntityType = policy.EntityType,
            RetentionPeriodMonths = policy.RetentionPeriodMonths,
            IsActive = policy.IsActive,
            AutoPurgeEnabled = policy.AutoPurgeEnabled,
            CreatedAt = policy.CreatedAt,
            UpdatedAt = policy.UpdatedAt,
            CreatedBy = policy.CreatedBy,
            UpdatedBy = policy.UpdatedBy
        };
    }
}

