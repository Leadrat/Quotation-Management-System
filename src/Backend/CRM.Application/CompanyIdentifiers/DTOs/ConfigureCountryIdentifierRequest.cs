using System;

namespace CRM.Application.CompanyIdentifiers.DTOs
{
    public class ConfigureCountryIdentifierRequest
    {
        public Guid CountryId { get; set; }
        public Guid IdentifierTypeId { get; set; }
        public bool IsRequired { get; set; } = false;
        public string? ValidationRegex { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public string? DisplayName { get; set; }
        public string? HelpText { get; set; }
        public int DisplayOrder { get; set; } = 0;
    }
}

