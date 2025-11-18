namespace CRM.Application.Localization.Dtos;

public class CurrencyConversionResponse
{
    public decimal OriginalAmount { get; set; }
    public string FromCurrencyCode { get; set; } = string.Empty;
    public decimal ConvertedAmount { get; set; }
    public string ToCurrencyCode { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
    public string? FormattedOriginalAmount { get; set; }
    public string? FormattedConvertedAmount { get; set; }
}


