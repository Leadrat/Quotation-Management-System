using System.Text.Json;

namespace CRM.Application.Admin.DTOs;

/// <summary>
/// DTO for audit log entry
/// </summary>
public class AuditLogDto
{
    public Guid Id { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public Guid PerformedBy { get; set; }
    public string? PerformedByEmail { get; set; }
    public string? IpAddress { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public JsonDocument? Changes { get; set; }
}

/// <summary>
/// DTO for paginated audit log response
/// </summary>
public class AuditLogListDto
{
    public List<AuditLogDto> Entries { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

