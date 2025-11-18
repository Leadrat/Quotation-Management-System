using System.Text.Json;

namespace CRM.Application.Admin.Services;

/// <summary>
/// Service for creating and querying audit log entries
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Creates a new audit log entry
    /// </summary>
    Task<Guid> LogAsync(
        string actionType,
        string entity,
        Guid? entityId,
        Guid performedBy,
        string? ipAddress,
        JsonDocument? changes = null);

    /// <summary>
    /// Gets audit log entries with optional filters
    /// </summary>
    Task<(List<CRM.Domain.Admin.AuditLog> Entries, int TotalCount)> GetAuditLogsAsync(
        Guid? performedBy = null,
        string? actionType = null,
        string? entity = null,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        int pageNumber = 1,
        int pageSize = 50);

    /// <summary>
    /// Gets a single audit log entry by ID
    /// </summary>
    Task<CRM.Domain.Admin.AuditLog?> GetByIdAsync(Guid id);
}

