using System;

namespace CRM.Domain.Entities;

public class UserPreferences
{
    public Guid UserId { get; set; }
    public string LanguageCode { get; set; } = "en";
    public string? CurrencyCode { get; set; }
    public string DateFormat { get; set; } = "dd/MM/yyyy";
    public string TimeFormat { get; set; } = "24h";
    public string NumberFormat { get; set; } = "en-IN";
    public string? Timezone { get; set; }
    public int FirstDayOfWeek { get; set; } = 1; // 1=Monday, 0=Sunday
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public Currency? Currency { get; set; }
    public SupportedLanguage? Language { get; set; }
}

