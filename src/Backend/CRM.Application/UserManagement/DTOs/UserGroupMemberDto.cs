using System;

namespace CRM.Application.UserManagement.DTOs;

public class UserGroupMemberDto
{
    public Guid GroupMemberId { get; set; }
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
}

