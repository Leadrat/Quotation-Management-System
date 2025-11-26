using System;
using CRM.Domain.Enums;

namespace CRM.Application.TaxManagement.Commands
{
    public class CreateCountryCommand
    {
        public string CountryName { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public TaxFrameworkType TaxFrameworkType { get; set; }
        public string DefaultCurrency { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool IsDefault { get; set; } = false;
        public Guid CreatedByUserId { get; set; }
    }
}

