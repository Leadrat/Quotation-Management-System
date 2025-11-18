using System;

namespace CRM.Application.Localization.Dtos;

public class UpdateExchangeRateRequest
{
    public string FromCurrencyCode { get; set; } = string.Empty;
    public string ToCurrencyCode { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Source { get; set; }
}


