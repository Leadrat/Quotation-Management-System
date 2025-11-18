namespace CRM.Application.Localization.Dtos;

public class CurrencyDto
{
    public string CurrencyCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public int DecimalPlaces { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
}

