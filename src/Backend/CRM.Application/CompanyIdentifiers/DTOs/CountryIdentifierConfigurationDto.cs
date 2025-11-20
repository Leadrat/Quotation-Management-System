using System;

namespace CRM.Application.CompanyIdentifiers.DTOs
{
    public class CountryIdentifierConfigurationDto
    {
        public Guid ConfigurationId { get; set; }
        public Guid CountryId { get; set; }
        public string? CountryName { get; set; }
        public Guid IdentifierTypeId { get; set; }
        public string? IdentifierTypeName { get; set; }
        public string? IdentifierTypeDisplayName { get; set; }
        public bool IsRequired { get; set; }
        public string? ValidationRegex { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public string? DisplayName { get; set; }
        public string? HelpText { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}

