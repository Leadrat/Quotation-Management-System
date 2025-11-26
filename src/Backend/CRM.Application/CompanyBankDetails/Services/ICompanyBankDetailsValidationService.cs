using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Application.CompanyBankDetails.DTOs;

namespace CRM.Application.CompanyBankDetails.Services
{
    public interface ICompanyBankDetailsValidationService
    {
        Task<Dictionary<string, List<string>>> ValidateAsync(Guid countryId, Dictionary<string, string> values);
        Task<List<CompanyBankFieldDto>> GetFieldsForCountryAsync(Guid countryId);
    }
}

