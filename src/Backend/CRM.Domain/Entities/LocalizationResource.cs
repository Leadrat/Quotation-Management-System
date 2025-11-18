using System;

namespace CRM.Domain.Entities;

public class LocalizationResource
{
    public Guid ResourceId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string ResourceKey { get; set; } = string.Empty;
    public string ResourceValue { get; set; } = string.Empty;
    public string? Category { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public Guid? UpdatedByUserId { get; set; }

    // Navigation properties
    public SupportedLanguage? Language { get; set; }
    public User? CreatedByUser { get; set; }
    public User? UpdatedByUser { get; set; }
}

