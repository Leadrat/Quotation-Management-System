using System;
using System.ComponentModel.DataAnnotations.Schema;
using CRM.Domain.Entities;

namespace CRM.Domain.UserManagement;

[Table("TeamMembers")]
public class TeamMember
{
    public Guid TeamMemberId { get; set; }
    public Guid TeamId { get; set; }
    public Guid UserId { get; set; }
    public DateTime JoinedAt { get; set; }
    public string Role { get; set; } = "Member"; // Member, Lead, Admin

    // Navigation properties
    public virtual Team Team { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}

