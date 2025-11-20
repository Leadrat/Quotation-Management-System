using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Application.CompanyIdentifiers.DTOs;

namespace CRM.Application.CompanyIdentifiers.Services
{
    public interface ICompanyIdentifierValidationService
    {
        Task<Dictionary<string, List<string>>> ValidateAsync(Guid countryId, Dictionary<string, string> values);
        Task<List<CompanyIdentifierFieldDto>> GetFieldsForCountryAsync(Guid countryId);
    }
}

