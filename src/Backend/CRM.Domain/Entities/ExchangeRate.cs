using System;

namespace CRM.Domain.Entities;

public class ExchangeRate
{
    public Guid ExchangeRateId { get; set; }
    public string FromCurrencyCode { get; set; } = string.Empty;
    public string ToCurrencyCode { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Source { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid? CreatedByUserId { get; set; }

    // Navigation properties
    public Currency? FromCurrency { get; set; }
    public Currency? ToCurrency { get; set; }
    public User? CreatedByUser { get; set; }
}

