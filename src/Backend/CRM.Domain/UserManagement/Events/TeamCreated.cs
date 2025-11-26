using System;

namespace CRM.Domain.UserManagement.Events;

public class TeamCreated
{
    public Guid TeamId { get; init; }
    public string Name { get; init; } = string.Empty;
    public Guid TeamLeadUserId { get; init; }
    public Guid? ParentTeamId { get; init; }
    public Guid CompanyId { get; init; }
    public DateTime CreatedAt { get; init; }
    public Guid CreatedByUserId { get; init; }
}

