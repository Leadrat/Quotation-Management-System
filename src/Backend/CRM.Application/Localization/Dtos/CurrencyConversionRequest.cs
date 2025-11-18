using System;

namespace CRM.Application.Localization.Dtos;

public class CurrencyConversionRequest
{
    public decimal Amount { get; set; }
    public string FromCurrencyCode { get; set; } = string.Empty;
    public string ToCurrencyCode { get; set; } = string.Empty;
    public DateTime? AsOfDate { get; set; }
}


