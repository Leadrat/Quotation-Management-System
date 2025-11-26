using System;

namespace CRM.Application.TaxManagement.Queries
{
    public class GetAllTaxRatesQuery
    {
        public Guid? JurisdictionId { get; set; }
        public Guid? TaxFrameworkId { get; set; }
        public Guid? ProductServiceCategoryId { get; set; }
        public DateOnly? AsOfDate { get; set; }
    }
}

