using CRM.Domain.Enums;

namespace CRM.Application.TaxManagement.Requests
{
    public class UpdateCountryRequest
    {
        public string? CountryName { get; set; }
        public string? CountryCode { get; set; }
        public TaxFrameworkType? TaxFrameworkType { get; set; }
        public string? DefaultCurrency { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDefault { get; set; }
    }
}

