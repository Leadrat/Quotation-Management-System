using System;
using System.Collections.Generic;

namespace CRM.Application.UserManagement.Commands;

public class BulkDeactivateUsersCommand
{
    public List<Guid> UserIds { get; set; } = new();
    public Guid DeactivatedByUserId { get; set; }
    public string RequestorRole { get; set; } = string.Empty;
}

