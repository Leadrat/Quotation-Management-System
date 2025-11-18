using System;
using CRM.Application.Localization.Dtos;

namespace CRM.Application.Localization.Queries;

public class ConvertCurrencyQuery
{
    public CurrencyConversionRequest Request { get; set; } = new();
}


