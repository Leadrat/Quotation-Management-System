using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.CompanyBankDetails.Commands;
using CRM.Application.CompanyBankDetails.DTOs;
using CRM.Application.CompanyBankDetails.Services;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.CompanyBankDetails.Commands.Handlers
{
    public class SaveCompanyBankDetailsCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly ICompanyBankDetailsValidationService _validationService;

        public SaveCompanyBankDetailsCommandHandler(
            IAppDbContext db,
            ICompanyBankDetailsValidationService validationService)
        {
            _db = db;
            _validationService = validationService;
        }

        public async Task<CompanyBankDetailsDto> Handle(SaveCompanyBankDetailsCommand command)
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
            var companyDetails = await _db.CompanyDetails
                .Include(c => c.BankDetails)
                .FirstOrDefaultAsync();
            
            if (companyDetails == null)
            {
                companyDetails = new Domain.Entities.CompanyDetails
                {
                    CompanyDetailsId = new Guid("00000000-0000-0000-0000-000000000001"),
                    CreatedAt = DateTimeOffset.UtcNow
                };
                _db.CompanyDetails.Add(companyDetails);
            }

            // Find or create bank details for the country
            var bankDetails = companyDetails.BankDetails
                .FirstOrDefault(b => b.CountryId == command.Request.CountryId);

            var now = DateTimeOffset.UtcNow;

            if (bankDetails == null)
            {
                // Create new bank details record
                bankDetails = new Domain.Entities.BankDetails
                {
                    BankDetailsId = Guid.NewGuid(),
                    CompanyDetailsId = companyDetails.CompanyDetailsId,
                    CountryId = command.Request.CountryId,
                    Country = string.Empty, // Keep for backward compatibility
                    AccountNumber = string.Empty, // Required field - set default
                    BankName = string.Empty, // Required field - set default
                    CreatedAt = now,
                    UpdatedAt = now,
                    UpdatedBy = command.UpdatedBy
                };
                _db.BankDetails.Add(bankDetails);
            }
            else
            {
                bankDetails.UpdatedAt = now;
                bankDetails.UpdatedBy = command.UpdatedBy;
            }

            // Serialize values to JSONB
            bankDetails.FieldValues = JsonSerializer.Serialize(command.Request.Values);

            await _db.SaveChangesAsync();

            // Return the updated DTO
            var fields = await _validationService.GetFieldsForCountryAsync(command.Request.CountryId);
            
            // Populate values
            foreach (var field in fields)
            {
                var bankFieldTypeIdStr = field.BankFieldTypeId.ToString();
                if (command.Request.Values.ContainsKey(bankFieldTypeIdStr))
                {
                    field.Value = command.Request.Values[bankFieldTypeIdStr];
                }
            }

            // Get country name
            var country = await _db.Countries
                .FirstOrDefaultAsync(c => c.CountryId == command.Request.CountryId);

            return new CompanyBankDetailsDto
            {
                CountryId = command.Request.CountryId,
                CountryName = country?.CountryName,
                Fields = fields
            };
        }
    }
}

