using System;
using CRM.Application.Localization.Dtos;

namespace CRM.Application.Localization.Commands;

public class UpdateCompanyPreferencesCommand
{
    public Guid CompanyId { get; set; }
    public UpdateCompanyPreferencesRequest Request { get; set; } = new();
    public Guid UpdatedByUserId { get; set; }
}


