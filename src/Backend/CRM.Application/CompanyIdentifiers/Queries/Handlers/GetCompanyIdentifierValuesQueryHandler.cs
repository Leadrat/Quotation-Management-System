using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.CompanyIdentifiers.DTOs;
using CRM.Application.CompanyIdentifiers.Queries;
using CRM.Application.CompanyIdentifiers.Services;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.CompanyIdentifiers.Queries.Handlers
{
    public class GetCompanyIdentifierValuesQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly ICompanyIdentifierValidationService _validationService;

        public GetCompanyIdentifierValuesQueryHandler(
            IAppDbContext db,
            ICompanyIdentifierValidationService validationService)
        {
            _db = db;
            _validationService = validationService;
        }

        public async Task<CompanyIdentifierValuesDto> Handle(GetCompanyIdentifierValuesQuery query)
        {
            // Get fields for the country
            var fields = await _validationService.GetFieldsForCountryAsync(query.CountryId);

            // Get company details (singleton)
            var companyDetails = await _db.CompanyDetails.FirstOrDefaultAsync();
            
            // Extract values from JSONB if they exist
            if (companyDetails != null && !string.IsNullOrWhiteSpace(companyDetails.IdentifierValues))
            {
                try
                {
                    var identifierValues = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(
                        companyDetails.IdentifierValues);

                    if (identifierValues != null)
                    {
                        var countryIdStr = query.CountryId.ToString();
                        if (identifierValues.ContainsKey(countryIdStr))
                        {
                            var countryValues = identifierValues[countryIdStr];
                            
                            // Populate field values
                            foreach (var field in fields)
                            {
                                var identifierTypeIdStr = field.IdentifierTypeId.ToString();
                                if (countryValues.ContainsKey(identifierTypeIdStr))
                                {
                                    field.Value = countryValues[identifierTypeIdStr];
                                }
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

            return new CompanyIdentifierValuesDto
            {
                CountryId = query.CountryId,
                CountryName = country?.CountryName,
                Fields = fields
            };
        }
    }
}

