using System;
using CRM.Domain.Enums;

namespace CRM.Application.TaxManagement.Dtos
{
    public class CountryDto
    {
        public Guid CountryId { get; set; }
        public string CountryName { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public TaxFrameworkType TaxFrameworkType { get; set; }
        public string DefaultCurrency { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}

