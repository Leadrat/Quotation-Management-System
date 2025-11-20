using System;

namespace CRM.Application.TaxManagement.Queries
{
    public class GetJurisdictionsByCountryQuery
    {
        public Guid CountryId { get; set; }
        public Guid? ParentJurisdictionId { get; set; }
        public bool? IsActive { get; set; }
    }
}

