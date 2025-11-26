using System;
using System.Collections.Generic;

namespace CRM.Application.UserManagement.DTOs;

public class TeamDto
{
    public Guid TeamId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid TeamLeadUserId { get; set; }
    public string TeamLeadName { get; set; } = string.Empty;
    public Guid? ParentTeamId { get; set; }
    public string? ParentTeamName { get; set; }
    public Guid CompanyId { get; set; }
    public bool IsActive { get; set; }
    public int MemberCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<TeamDto> ChildTeams { get; set; } = new();
}

