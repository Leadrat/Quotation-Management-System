using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.CompanyBankDetails.DTOs;
using CRM.Application.CompanyBankDetails.Queries;
using CRM.Application.CompanyBankDetails.Services;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.CompanyBankDetails.Queries.Handlers
{
    public class GetCompanyBankDetailsQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly ICompanyBankDetailsValidationService _validationService;

        public GetCompanyBankDetailsQueryHandler(
            IAppDbContext db,
            ICompanyBankDetailsValidationService validationService)
        {
            _db = db;
            _validationService = validationService;
        }

        public async Task<CompanyBankDetailsDto> Handle(GetCompanyBankDetailsQuery query)
        {
            // Get fields for the country
            var fields = await _validationService.GetFieldsForCountryAsync(query.CountryId);

            // Get company details (singleton) and find bank details for the country
            var companyDetails = await _db.CompanyDetails
                .Include(c => c.BankDetails)
                .FirstOrDefaultAsync();

            // Find bank details for the country
            var bankDetails = companyDetails?.BankDetails
                .FirstOrDefault(b => b.CountryId == query.CountryId);

            // Extract values from JSONB FieldValues if they exist
            if (bankDetails != null && !string.IsNullOrWhiteSpace(bankDetails.FieldValues))
            {
                try
                {
                    var fieldValues = JsonSerializer.Deserialize<Dictionary<string, string>>(
                        bankDetails.FieldValues);

                    if (fieldValues != null)
                    {
                        // Populate field values
                        foreach (var field in fields)
                        {
                            var bankFieldTypeIdStr = field.BankFieldTypeId.ToString();
                            if (fieldValues.ContainsKey(bankFieldTypeIdStr))
                            {
                                field.Value = fieldValues[bankFieldTypeIdStr];
                            }
                        }
                    }
                }
                catch
                {
                    // JSON deserialization failed - continue with empty values
                }
            }

            // Get country name
            var country = await _db.Countries
                .FirstOrDefaultAsync(c => c.CountryId == query.CountryId);

            return new CompanyBankDetailsDto
            {
                CountryId = query.CountryId,
                CountryName = country?.CountryName,
                Fields = fields
            };
        }
    }
}

