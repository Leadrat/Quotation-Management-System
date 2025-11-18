namespace CRM.Application.Localization.Dtos;

public class UpdateCurrencyRequest
{
    public string? DisplayName { get; set; }
    public string? Symbol { get; set; }
    public int? DecimalPlaces { get; set; }
    public bool? IsDefault { get; set; }
    public bool? IsActive { get; set; }
}


