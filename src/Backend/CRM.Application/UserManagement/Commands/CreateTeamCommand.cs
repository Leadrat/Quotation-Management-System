using System;
using CRM.Application.UserManagement.Requests;

namespace CRM.Application.UserManagement.Commands;

public class CreateTeamCommand
{
    public CreateTeamRequest Request { get; set; } = null!;
    public Guid CreatedByUserId { get; set; }
    public string RequestorRole { get; set; } = string.Empty;
}

