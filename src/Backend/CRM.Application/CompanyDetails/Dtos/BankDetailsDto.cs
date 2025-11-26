using System;

namespace CRM.Application.CompanyDetails.Dtos
{
    public class BankDetailsDto
    {
        public Guid BankDetailsId { get; set; }
        public string Country { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string? IfscCode { get; set; }
        public string? Iban { get; set; }
        public string? SwiftCode { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string? BranchName { get; set; }
    }
}

