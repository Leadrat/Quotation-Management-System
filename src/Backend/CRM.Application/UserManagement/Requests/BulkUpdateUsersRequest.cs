using System;
using System.Collections.Generic;

namespace CRM.Application.UserManagement.Requests;

public class BulkUpdateUsersRequest
{
    public List<Guid> UserIds { get; set; } = new();
    public bool? IsActive { get; set; }
    public Guid? RoleId { get; set; }
    public Guid? TeamId { get; set; }
}

