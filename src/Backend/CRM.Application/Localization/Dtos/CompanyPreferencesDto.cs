namespace CRM.Application.Localization.Dtos;

public class CompanyPreferencesDto
{
    public Guid CompanyId { get; set; }
    public string DefaultLanguageCode { get; set; } = "en";
    public string DefaultCurrencyCode { get; set; } = "INR";
    public string DateFormat { get; set; } = "dd/MM/yyyy";
    public string TimeFormat { get; set; } = "24h";
    public string NumberFormat { get; set; } = "en-IN";
    public string? Timezone { get; set; }
    public int FirstDayOfWeek { get; set; } = 1;
}


