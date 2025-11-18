using System;

namespace CRM.Domain.Entities;

public class Currency
{
    public string CurrencyCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public int DecimalPlaces { get; set; } = 2;
    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public Guid? UpdatedByUserId { get; set; }

    // Navigation properties
    public User? CreatedByUser { get; set; }
    public User? UpdatedByUser { get; set; }
}

