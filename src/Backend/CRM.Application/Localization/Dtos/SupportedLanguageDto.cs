namespace CRM.Application.Localization.Dtos;

public class SupportedLanguageDto
{
    public string LanguageCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string DisplayNameEn { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
    public bool IsRTL { get; set; }
    public bool IsActive { get; set; }
    public string? FlagIcon { get; set; }
}


