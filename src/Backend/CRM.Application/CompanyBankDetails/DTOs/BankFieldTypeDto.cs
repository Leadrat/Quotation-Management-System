using System;

namespace CRM.Application.CompanyBankDetails.DTOs
{
    public class BankFieldTypeDto
    {
        public Guid BankFieldTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}

