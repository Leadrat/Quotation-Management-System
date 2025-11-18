using System.Text.Json;
using CRM.Application.Common.Persistence;
using CRM.Domain.Admin;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Admin.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IAppDbContext _db;

    public AuditLogService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Guid> LogAsync(
        string actionType,
        string entity,
        Guid? entityId,
        Guid performedBy,
        string? ipAddress,
        JsonDocument? changes = null)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            ActionType = actionType,
            Entity = entity,
            EntityId = entityId,
            PerformedBy = performedBy,
            IpAddress = ipAddress,
            Timestamp = DateTimeOffset.UtcNow,
            Changes = changes
        };

        _db.AuditLogs.Add(auditLog);
        await _db.SaveChangesAsync();

        return auditLog.Id;
    }

    public async Task<(List<AuditLog> Entries, int TotalCount)> GetAuditLogsAsync(
        Guid? performedBy = null,
        string? actionType = null,
        string? entity = null,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        var query = _db.AuditLogs
            .Include(a => a.PerformedByUser)
            .AsQueryable();

        if (performedBy.HasValue)
        {
            query = query.Where(a => a.PerformedBy == performedBy.Value);
        }

        if (!string.IsNullOrWhiteSpace(actionType))
        {
            query = query.Where(a => a.ActionType == actionType);
        }

        if (!string.IsNullOrWhiteSpace(entity))
        {
            query = query.Where(a => a.Entity == entity);
        }

        if (startDate.HasValue)
        {
            query = query.Where(a => a.Timestamp >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(a => a.Timestamp <= endDate.Value);
        }

        var totalCount = await query.CountAsync();

        var entries = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (entries, totalCount);
    }

    public async Task<AuditLog?> GetByIdAsync(Guid id)
    {
        return await _db.AuditLogs
            .Include(a => a.PerformedByUser)
            .FirstOrDefaultAsync(a => a.Id == id);
    }
}

