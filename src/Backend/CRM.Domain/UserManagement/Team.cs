using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using CRM.Domain.Entities;

namespace CRM.Domain.UserManagement;

[Table("Teams")]
public class Team
{
    public Guid TeamId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid TeamLeadUserId { get; set; }
    public Guid? ParentTeamId { get; set; }
    public Guid CompanyId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public virtual User TeamLead { get; set; } = null!;
    public virtual Team? ParentTeam { get; set; }
    public virtual ICollection<Team> ChildTeams { get; set; } = new List<Team>();
    public virtual ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();

    public bool IsActiveTeam() => IsActive;
}

