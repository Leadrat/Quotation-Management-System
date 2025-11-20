using System;
using System.Collections.Generic;

namespace CRM.Application.CompanyIdentifiers.DTOs
{
    public class SaveCompanyIdentifierValuesRequest
    {
        public Guid CountryId { get; set; }
        public Dictionary<string, string> Values { get; set; } = new(); // Key: IdentifierTypeId (as string), Value: Identifier value
    }
}

