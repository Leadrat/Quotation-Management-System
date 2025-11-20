using System;
using System.Collections.Generic;

namespace CRM.Application.CompanyBankDetails.DTOs
{
    public class CompanyBankDetailsDto
    {
        public Guid CountryId { get; set; }
        public string? CountryName { get; set; }
        public List<CompanyBankFieldDto> Fields { get; set; } = new();
    }

    public class CompanyBankFieldDto
    {
        public Guid BankFieldTypeId { get; set; }
        public string BankFieldTypeName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Value { get; set; }
        public bool IsRequired { get; set; }
        public string? ValidationRegex { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public string? HelpText { get; set; }
        public int DisplayOrder { get; set; }
    }
}

