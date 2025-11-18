namespace CRM.Application.Localization.Dtos;

public class UserPreferencesDto
{
    public Guid UserId { get; set; }
    public string LanguageCode { get; set; } = "en";
    public string? CurrencyCode { get; set; }
    public string DateFormat { get; set; } = "dd/MM/yyyy";
    public string TimeFormat { get; set; } = "24h";
    public string NumberFormat { get; set; } = "en-IN";
    public string? Timezone { get; set; }
    public int FirstDayOfWeek { get; set; } = 1;
}


