using System;

namespace CRM.Application.TaxManagement.Queries
{
    public class GetAllTaxFrameworksQuery
    {
        public Guid? CountryId { get; set; }
        public bool? IsActive { get; set; }
    }
}

