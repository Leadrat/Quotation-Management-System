using System;

namespace CRM.Application.UserManagement.Queries;

public class GetTeamsQuery
{
    public Guid? CompanyId { get; set; }
    public Guid? TeamLeadUserId { get; set; }
    public bool? IsActive { get; set; }
    public Guid? ParentTeamId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public Guid RequestorUserId { get; set; }
    public string RequestorRole { get; set; } = string.Empty;
}

