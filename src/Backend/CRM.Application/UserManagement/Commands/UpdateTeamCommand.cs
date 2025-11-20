using System;
using CRM.Application.UserManagement.Requests;

namespace CRM.Application.UserManagement.Commands;

public class UpdateTeamCommand
{
    public Guid TeamId { get; set; }
    public UpdateTeamRequest Request { get; set; } = null!;
    public Guid UpdatedByUserId { get; set; }
    public string RequestorRole { get; set; } = string.Empty;
}

