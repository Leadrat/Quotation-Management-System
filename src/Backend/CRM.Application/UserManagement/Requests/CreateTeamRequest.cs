using System;

namespace CRM.Application.UserManagement.Requests;

public class CreateTeamRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid TeamLeadUserId { get; set; }
    public Guid? ParentTeamId { get; set; }
    public Guid CompanyId { get; set; }
}

