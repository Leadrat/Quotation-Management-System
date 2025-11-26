using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.CompanyBankDetails.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.CompanyBankDetails.Services
{
    public class CompanyBankDetailsValidationService : ICompanyBankDetailsValidationService
    {
        private readonly IAppDbContext _db;

        public CompanyBankDetailsValidationService(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<Dictionary<string, List<string>>> ValidateAsync(Guid countryId, Dictionary<string, string> values)
        {
            var errors = new Dictionary<string, List<string>>();

            // Get all active configurations for the country
            var configurations = await _db.CountryBankFieldConfigurations
                .Include(c => c.BankFieldType)
                .Where(c => c.CountryId == countryId 
                    && c.IsActive 
                    && c.DeletedAt == null
                    && c.BankFieldType != null 
                    && c.BankFieldType.DeletedAt == null)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            foreach (var config in configurations)
            {
                var bankFieldTypeIdStr = config.BankFieldTypeId.ToString();
                var value = values.ContainsKey(bankFieldTypeIdStr) ? values[bankFieldTypeIdStr] : null;

                var fieldErrors = new List<string>();

                // Check required
                if (config.IsRequired && string.IsNullOrWhiteSpace(value))
                {
                    fieldErrors.Add($"{config.DisplayName ?? config.BankFieldType?.DisplayName} is required");
                }

                // If value is provided, validate it
                if (!string.IsNullOrWhiteSpace(value))
                {
                    // Validate length
                    if (config.MinLength.HasValue && value.Length < config.MinLength.Value)
                    {
                        fieldErrors.Add($"{config.DisplayName ?? config.BankFieldType?.DisplayName} must be at least {config.MinLength.Value} characters");
                    }

                    if (config.MaxLength.HasValue && value.Length > config.MaxLength.Value)
                    {
                        fieldErrors.Add($"{config.DisplayName ?? config.BankFieldType?.DisplayName} must be no more than {config.MaxLength.Value} characters");
                    }

                    // Validate regex
                    if (!string.IsNullOrWhiteSpace(config.ValidationRegex))
                    {
                        try
                        {
                            var regex = new Regex(config.ValidationRegex);
                            if (!regex.IsMatch(value))
                            {
                                fieldErrors.Add($"{config.DisplayName ?? config.BankFieldType?.DisplayName} format is invalid");
                            }
                        }
                        catch
                        {
                            // Invalid regex pattern - skip this validation
                        }
                    }
                }

                if (fieldErrors.Any())
                {
                    errors[bankFieldTypeIdStr] = fieldErrors;
                }
            }

            return errors;
        }

        public async Task<List<CompanyBankFieldDto>> GetFieldsForCountryAsync(Guid countryId)
        {
            var configurations = await _db.CountryBankFieldConfigurations
                .Include(c => c.BankFieldType)
                .Where(c => c.CountryId == countryId 
                    && c.IsActive 
                    && c.DeletedAt == null
                    && c.BankFieldType != null 
                    && c.BankFieldType.DeletedAt == null)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return configurations.Select(c => new CompanyBankFieldDto
            {
                BankFieldTypeId = c.BankFieldTypeId,
                BankFieldTypeName = c.BankFieldType!.Name,
                DisplayName = c.DisplayName ?? c.BankFieldType.DisplayName,
                IsRequired = c.IsRequired,
                ValidationRegex = c.ValidationRegex,
                MinLength = c.MinLength,
                MaxLength = c.MaxLength,
                HelpText = c.HelpText,
                DisplayOrder = c.DisplayOrder
            }).ToList();
        }
    }
}

