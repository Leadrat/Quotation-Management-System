using System.Collections.Generic;

namespace CRM.Application.CompanyDetails.Dtos
{
    public class UpdateCompanyDetailsRequest
    {
        public string? PanNumber { get; set; }
        public string? TanNumber { get; set; }
        public string? GstNumber { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? Website { get; set; }
        public string? LegalDisclaimer { get; set; }
        public string? LogoUrl { get; set; }
        public Guid? CountryId { get; set; } // Country ID for country-specific storage
        public List<BankDetailsDto> BankDetails { get; set; } = new();
    }
}

