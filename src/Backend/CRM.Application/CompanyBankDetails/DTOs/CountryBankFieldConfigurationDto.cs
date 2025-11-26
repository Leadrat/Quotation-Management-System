using System;

namespace CRM.Application.CompanyBankDetails.DTOs
{
    public class CountryBankFieldConfigurationDto
    {
        public Guid ConfigurationId { get; set; }
        public Guid CountryId { get; set; }
        public string? CountryName { get; set; }
        public Guid BankFieldTypeId { get; set; }
        public string? BankFieldTypeName { get; set; }
        public string? BankFieldTypeDisplayName { get; set; }
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

