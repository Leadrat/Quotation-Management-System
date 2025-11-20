using System;

namespace CRM.Application.TaxManagement.Queries
{
    public class GetTaxCalculationLogQuery
    {
        public Guid? QuotationId { get; set; }
        public Guid? CountryId { get; set; }
        public Guid? JurisdictionId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}

