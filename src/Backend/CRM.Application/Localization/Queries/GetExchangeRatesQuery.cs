using System;

namespace CRM.Application.Localization.Queries;

public class GetExchangeRatesQuery
{
    public string? FromCurrencyCode { get; set; }
    public string? ToCurrencyCode { get; set; }
    public DateTime? AsOfDate { get; set; }
}


