using System;
using CRM.Application.Localization.Dtos;

namespace CRM.Application.Localization.Commands;

public class UpdateUserPreferencesCommand
{
    public Guid UserId { get; set; }
    public UpdateUserPreferencesRequest Request { get; set; } = new();
}


