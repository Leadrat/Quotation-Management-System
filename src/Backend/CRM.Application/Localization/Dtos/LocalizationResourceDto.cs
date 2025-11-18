using System;

namespace CRM.Application.Localization.Dtos;

public class LocalizationResourceDto
{
    public Guid ResourceId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string ResourceKey { get; set; } = string.Empty;
    public string ResourceValue { get; set; } = string.Empty;
    public string? Category { get; set; }
    public bool IsActive { get; set; }
}


