using System;
using System.Collections.Generic;

namespace CRM.Application.CompanyBankDetails.Queries
{
    public class GetCountryBankFieldConfigurationsQuery
    {
        public Guid? CountryId { get; set; }
        public Guid? BankFieldTypeId { get; set; }
        public bool IncludeInactive { get; set; } = false;
    }
}

