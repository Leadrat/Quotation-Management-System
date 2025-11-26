using System;

namespace CRM.Application.TaxManagement.Queries
{
    public class GetTaxRatesByJurisdictionQuery
    {
        public Guid JurisdictionId { get; set; }
        public Guid? ProductServiceCategoryId { get; set; }
        public DateOnly? AsOfDate { get; set; }
    }
}

