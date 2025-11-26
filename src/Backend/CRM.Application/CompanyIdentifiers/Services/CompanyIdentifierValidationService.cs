using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.CompanyIdentifiers.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.CompanyIdentifiers.Services
{
    public class CompanyIdentifierValidationService : ICompanyIdentifierValidationService
    {
        private readonly IAppDbContext _db;

        public CompanyIdentifierValidationService(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<Dictionary<string, List<string>>> ValidateAsync(Guid countryId, Dictionary<string, string> values)
        {
            var errors = new Dictionary<string, List<string>>();

            // Get all active configurations for the country
            var configurations = await _db.CountryIdentifierConfigurations
                .Include(c => c.IdentifierType)
                .Where(c => c.CountryId == countryId 
                    && c.IsActive 
                    && c.DeletedAt == null
                    && c.IdentifierType != null 
                    && c.IdentifierType.DeletedAt == null)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            foreach (var config in configurations)
            {
                var identifierTypeIdStr = config.IdentifierTypeId.ToString();
                var value = values.ContainsKey(identifierTypeIdStr) ? values[identifierTypeIdStr] : null;

                var fieldErrors = new List<string>();

                // Check required
                if (config.IsRequired && string.IsNullOrWhiteSpace(value))
                {
                    fieldErrors.Add($"{config.DisplayName ?? config.IdentifierType?.DisplayName} is required");
                }

                // If value is provided, validate it
                if (!string.IsNullOrWhiteSpace(value))
                {
                    // Validate length
                    if (config.MinLength.HasValue && value.Length < config.MinLength.Value)
                    {
                        fieldErrors.Add($"{config.DisplayName ?? config.IdentifierType?.DisplayName} must be at least {config.MinLength.Value} characters");
                    }

                    if (config.MaxLength.HasValue && value.Length > config.MaxLength.Value)
                    {
                        fieldErrors.Add($"{config.DisplayName ?? config.IdentifierType?.DisplayName} must be no more than {config.MaxLength.Value} characters");
                    }

                    // Validate regex
                    if (!string.IsNullOrWhiteSpace(config.ValidationRegex))
                    {
                        try
                        {
                            var regex = new Regex(config.ValidationRegex);
                            if (!regex.IsMatch(value))
                            {
                                fieldErrors.Add($"{config.DisplayName ?? config.IdentifierType?.DisplayName} format is invalid");
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
                    errors[identifierTypeIdStr] = fieldErrors;
                }
            }

            return errors;
        }

        public async Task<List<CompanyIdentifierFieldDto>> GetFieldsForCountryAsync(Guid countryId)
        {
            var configurations = await _db.CountryIdentifierConfigurations
                .Include(c => c.IdentifierType)
                .Where(c => c.CountryId == countryId 
                    && c.IsActive 
                    && c.DeletedAt == null
                    && c.IdentifierType != null 
                    && c.IdentifierType.DeletedAt == null)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return configurations.Select(c => new CompanyIdentifierFieldDto
            {
                IdentifierTypeId = c.IdentifierTypeId,
                IdentifierTypeName = c.IdentifierType!.Name,
                DisplayName = c.DisplayName ?? c.IdentifierType.DisplayName,
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

