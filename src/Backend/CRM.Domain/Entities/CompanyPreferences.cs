using System;

namespace CRM.Domain.Entities;

public class CompanyPreferences
{
    public Guid CompanyId { get; set; }
    public string DefaultLanguageCode { get; set; } = "en";
    public string DefaultCurrencyCode { get; set; } = "INR";
    public string DateFormat { get; set; } = "dd/MM/yyyy";
    public string TimeFormat { get; set; } = "24h";
    public string NumberFormat { get; set; } = "en-IN";
    public string? Timezone { get; set; }
    public int FirstDayOfWeek { get; set; } = 1;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid? UpdatedByUserId { get; set; }

    // Navigation properties
    // Note: Company entity not yet created - navigation property will be added when Company entity exists
    public Currency? DefaultCurrency { get; set; }
    public SupportedLanguage? DefaultLanguage { get; set; }
    public User? UpdatedByUser { get; set; }
}

