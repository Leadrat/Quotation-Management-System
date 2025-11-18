using System;
using CRM.Application.Localization.Dtos;

namespace CRM.Application.Localization.Commands;

public class CreateLocalizationResourceCommand
{
    public CreateLocalizationResourceRequest Request { get; set; } = new();
    public Guid CreatedByUserId { get; set; }
}


