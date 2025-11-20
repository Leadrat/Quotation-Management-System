using System;
using System.Collections.Generic;

namespace CRM.Application.CompanyIdentifiers.Queries
{
    public class GetCountryIdentifierConfigurationsQuery
    {
        public Guid? CountryId { get; set; }
        public Guid? IdentifierTypeId { get; set; }
        public bool IncludeInactive { get; set; } = false;
    }
}

