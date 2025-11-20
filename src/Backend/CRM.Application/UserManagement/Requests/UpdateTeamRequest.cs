using System;

namespace CRM.Application.UserManagement.Requests;

public class UpdateTeamRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public Guid? TeamLeadUserId { get; set; }
    public Guid? ParentTeamId { get; set; }
    public bool? IsActive { get; set; }
}

