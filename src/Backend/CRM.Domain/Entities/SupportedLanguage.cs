using System;

namespace CRM.Domain.Entities;

public class SupportedLanguage
{
    public string LanguageCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string DisplayNameEn { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
    public bool IsRTL { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public string? FlagIcon { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

