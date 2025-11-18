using System;
using CRM.Application.Localization.Dtos;

namespace CRM.Application.Localization.Commands;

public class CreateCurrencyCommand
{
    public CreateCurrencyRequest Request { get; set; } = new();
    public Guid CreatedByUserId { get; set; }
}


