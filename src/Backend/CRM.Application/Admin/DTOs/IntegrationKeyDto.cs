namespace CRM.Application.Admin.DTOs;

/// <summary>
/// DTO for integration key (without decrypted value)
/// </summary>
public class IntegrationKeyDto
{
    public Guid Id { get; set; }
    public string KeyName { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? LastUsedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}

/// <summary>
/// DTO for showing decrypted key value (temporary, for display only)
/// </summary>
public class IntegrationKeyWithValueDto : IntegrationKeyDto
{
    public string KeyValue { get; set; } = string.Empty; // Decrypted value
}

