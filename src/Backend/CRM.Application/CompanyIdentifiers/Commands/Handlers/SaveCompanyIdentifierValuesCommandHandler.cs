using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.CompanyIdentifiers.Commands;
using CRM.Application.CompanyIdentifiers.DTOs;
using CRM.Application.CompanyIdentifiers.Services;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.CompanyIdentifiers.Commands.Handlers
{
    public class SaveCompanyIdentifierValuesCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly ICompanyIdentifierValidationService _validationService;

        public SaveCompanyIdentifierValuesCommandHandler(
            IAppDbContext db,
            ICompanyIdentifierValidationService validationService)
        {
            _db = db;
            _validationService = validationService;
        }

        public async Task<CompanyIdentifierValuesDto> Handle(SaveCompanyIdentifierValuesCommand command)
        {
            // Validate the values against country configuration
            var validationErrors = await _validationService.ValidateAsync(
                command.Request.CountryId, 
                command.Request.Values);

            if (validationErrors.Any())
            {
                var errorMessages = validationErrors
                    .SelectMany(e => e.Value.Select(msg => $"{e.Key}: {msg}"))
                    .ToList();
                throw new InvalidOperationException($"Validation failed: {string.Join("; ", errorMessages)}");
            }

            // Get or create company details (singleton)
            var companyDetails = await _db.CompanyDetails.FirstOrDefaultAsync();
            if (companyDetails == null)
            {
                companyDetails = new Domain.Entities.CompanyDetails
                {
                    CompanyDetailsId = new Guid("00000000-0000-0000-0000-000000000001"),
                    CreatedAt = DateTimeOffset.UtcNow
                };
                _db.CompanyDetails.Add(companyDetails);
            }

            // Deserialize existing IdentifierValues JSONB or create new dictionary
            Dictionary<string, Dictionary<string, string>> identifierValues;
            if (!string.IsNullOrWhiteSpace(companyDetails.IdentifierValues))
            {
                try
                {
                    identifierValues = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(
                        companyDetails.IdentifierValues) ?? new Dictionary<string, Dictionary<string, string>>();
                }
                catch
                {
                    identifierValues = new Dictionary<string, Dictionary<string, string>>();
                }
            }
            else
            {
                identifierValues = new Dictionary<string, Dictionary<string, string>>();
            }

            // Update or add values for this country
            var countryIdStr = command.Request.CountryId.ToString();
            identifierValues[countryIdStr] = command.Request.Values;

            // Serialize back to JSONB
            companyDetails.IdentifierValues = JsonSerializer.Serialize(identifierValues);
            companyDetails.UpdatedAt = DateTimeOffset.UtcNow;
            companyDetails.UpdatedBy = command.UpdatedBy;

            await _db.SaveChangesAsync();

            // Return the updated DTO
            var fields = await _validationService.GetFieldsForCountryAsync(command.Request.CountryId);
            
            // Populate values
            foreach (var field in fields)
            {
                var identifierTypeIdStr = field.IdentifierTypeId.ToString();
                if (command.Request.Values.ContainsKey(identifierTypeIdStr))
                {
                    field.Value = command.Request.Values[identifierTypeIdStr];
                }
            }

            // Get country name
            var country = await _db.Countries
                .FirstOrDefaultAsync(c => c.CountryId == command.Request.CountryId);

            return new CompanyIdentifierValuesDto
            {
                CountryId = command.Request.CountryId,
                CountryName = country?.CountryName,
                Fields = fields
            };
        }
    }
}

