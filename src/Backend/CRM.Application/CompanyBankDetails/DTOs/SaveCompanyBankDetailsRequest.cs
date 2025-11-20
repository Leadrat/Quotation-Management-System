using System;
using System.Collections.Generic;

namespace CRM.Application.CompanyBankDetails.DTOs
{
    public class SaveCompanyBankDetailsRequest
    {
        public Guid CountryId { get; set; }
        public Dictionary<string, string> Values { get; set; } = new(); // Key: BankFieldTypeId (as string), Value: Field value
    }
}

