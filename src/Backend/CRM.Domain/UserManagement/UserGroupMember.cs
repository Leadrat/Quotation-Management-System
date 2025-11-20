using System;
using System.ComponentModel.DataAnnotations.Schema;
using CRM.Domain.Entities;

namespace CRM.Domain.UserManagement;

[Table("UserGroupMembers")]
public class UserGroupMember
{
    public Guid GroupMemberId { get; set; }
    public Guid GroupId { get; set; }
    public Guid UserId { get; set; }
    public DateTime AddedAt { get; set; }

    // Navigation properties
    public virtual UserGroup UserGroup { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}

