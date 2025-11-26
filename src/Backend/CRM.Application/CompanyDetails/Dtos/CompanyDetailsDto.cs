using System;
using System.Collections.Generic;
using CRM.Application.CompanyIdentifiers.DTOs;
using CRM.Application.CompanyBankDetails.DTOs;

namespace CRM.Application.CompanyDetails.Dtos
{
    public class CompanyDetailsDto
    {
        public Guid CompanyDetailsId { get; set; }
        public string? PanNumber { get; set; }
        public string? TanNumber { get; set; }
        public string? GstNumber { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public Guid? CountryId { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? Website { get; set; }
        public string? LegalDisclaimer { get; set; }
        public string? LogoUrl { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public List<BankDetailsDto> BankDetails { get; set; } = new();
        
        // Dynamic country-specific fields
        public List<CompanyIdentifierFieldDto> IdentifierFields { get; set; } = new();
        public List<CompanyBankFieldDto> BankFields { get; set; } = new();
    }
}

