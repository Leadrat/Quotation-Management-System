using System;
using CRM.Application.Localization.Dtos;

namespace CRM.Application.Localization.Commands;

public class UpdateLocalizationResourceCommand
{
    public Guid ResourceId { get; set; }
    public UpdateLocalizationResourceRequest Request { get; set; } = new();
    public Guid UpdatedByUserId { get; set; }
}


