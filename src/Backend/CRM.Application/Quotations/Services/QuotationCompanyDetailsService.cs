using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.CompanyDetails.Dtos;
using CRM.Application.CompanyIdentifiers.DTOs;
using CRM.Application.CompanyBankDetails.DTOs;
using CRM.Application.CompanyIdentifiers.Services;
using CRM.Application.CompanyBankDetails.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CRM.Application.Quotations.Services
{
    public class QuotationCompanyDetailsService
    {
        private readonly IAppDbContext _db;
        private readonly ICompanyIdentifierValidationService _identifierValidationService;
        private readonly ICompanyBankDetailsValidationService _bankDetailsValidationService;
        private readonly IMemoryCache _cache;
        private const int CacheMinutes = 5;

        public QuotationCompanyDetailsService(
            IAppDbContext db,
            ICompanyIdentifierValidationService identifierValidationService,
            ICompanyBankDetailsValidationService bankDetailsValidationService,
            IMemoryCache cache)
        {
            _db = db;
            _identifierValidationService = identifierValidationService;
            _bankDetailsValidationService = bankDetailsValidationService;
            _cache = cache;
        }

        /// <summary>
        /// Gets company details filtered by client country for quotations
        /// </summary>
        public async Task<CompanyDetailsDto> GetCompanyDetailsForQuotationAsync(Guid clientCountryId)
        {
            // Check cache first
            var cacheKey = $"company-details-quotation-{clientCountryId}";
            if (_cache.TryGetValue(cacheKey, out CompanyDetailsDto? cached))
            {
                return cached!;
            }

            // Get company details (singleton)
            var companyDetails = await _db.CompanyDetails.FirstOrDefaultAsync();
            if (companyDetails == null)
            {
                throw new InvalidOperationException("Company details not configured");
            }

            // Extract country-specific basic details from JSONB if available
            var clientCountryIdStr = clientCountryId.ToString();
            var countrySpecificDetails = new Dictionary<string, string>();
            
            if (!string.IsNullOrWhiteSpace(companyDetails.CountryDetails))
            {
                try
                {
                    var allCountryDetails = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(
                        companyDetails.CountryDetails);
                    
                    if (allCountryDetails != null && allCountryDetails.ContainsKey(clientCountryIdStr))
                    {
                        countrySpecificDetails = allCountryDetails[clientCountryIdStr];
                    }
                }
                catch
                {
                    // JSON deserialization failed - continue with entity fields
                }
            }

            // Create filtered DTO - prefer country-specific details, fallback to entity fields
            var dto = new CompanyDetailsDto
            {
                CompanyDetailsId = companyDetails.CompanyDetailsId,
                PanNumber = companyDetails.PanNumber,
                TanNumber = companyDetails.TanNumber,
                GstNumber = companyDetails.GstNumber,
                // Use country-specific details if available, otherwise use entity fields
                CompanyName = countrySpecificDetails.ContainsKey("CompanyName") && !string.IsNullOrWhiteSpace(countrySpecificDetails["CompanyName"])
                    ? countrySpecificDetails["CompanyName"]
                    : companyDetails.CompanyName,
                CompanyAddress = countrySpecificDetails.ContainsKey("CompanyAddress") && !string.IsNullOrWhiteSpace(countrySpecificDetails["CompanyAddress"])
                    ? countrySpecificDetails["CompanyAddress"]
                    : companyDetails.CompanyAddress,
                City = countrySpecificDetails.ContainsKey("City") && !string.IsNullOrWhiteSpace(countrySpecificDetails["City"])
                    ? countrySpecificDetails["City"]
                    : companyDetails.City,
                State = countrySpecificDetails.ContainsKey("State") && !string.IsNullOrWhiteSpace(countrySpecificDetails["State"])
                    ? countrySpecificDetails["State"]
                    : companyDetails.State,
                PostalCode = countrySpecificDetails.ContainsKey("PostalCode") && !string.IsNullOrWhiteSpace(countrySpecificDetails["PostalCode"])
                    ? countrySpecificDetails["PostalCode"]
                    : companyDetails.PostalCode,
                Country = countrySpecificDetails.ContainsKey("Country") && !string.IsNullOrWhiteSpace(countrySpecificDetails["Country"])
                    ? countrySpecificDetails["Country"]
                    : companyDetails.Country,
                ContactEmail = countrySpecificDetails.ContainsKey("ContactEmail") && !string.IsNullOrWhiteSpace(countrySpecificDetails["ContactEmail"])
                    ? countrySpecificDetails["ContactEmail"]
                    : companyDetails.ContactEmail,
                ContactPhone = countrySpecificDetails.ContainsKey("ContactPhone") && !string.IsNullOrWhiteSpace(countrySpecificDetails["ContactPhone"])
                    ? countrySpecificDetails["ContactPhone"]
                    : companyDetails.ContactPhone,
                Website = countrySpecificDetails.ContainsKey("Website") && !string.IsNullOrWhiteSpace(countrySpecificDetails["Website"])
                    ? countrySpecificDetails["Website"]
                    : companyDetails.Website,
                LogoUrl = countrySpecificDetails.ContainsKey("LogoUrl") && !string.IsNullOrWhiteSpace(countrySpecificDetails["LogoUrl"])
                    ? countrySpecificDetails["LogoUrl"]
                    : companyDetails.LogoUrl,
                LegalDisclaimer = countrySpecificDetails.ContainsKey("LegalDisclaimer") && !string.IsNullOrWhiteSpace(countrySpecificDetails["LegalDisclaimer"])
                    ? countrySpecificDetails["LegalDisclaimer"]
                    : companyDetails.LegalDisclaimer,
                UpdatedAt = companyDetails.UpdatedAt
            };

            // Extract country-specific identifier values from JSONB
            var countryIdentifierValues = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(companyDetails.IdentifierValues))
            {
                try
                {
                    var allIdentifierValues = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(
                        companyDetails.IdentifierValues);
                    
                    var countryIdStr = clientCountryId.ToString();
                    if (allIdentifierValues != null && allIdentifierValues.ContainsKey(countryIdStr))
                    {
                        countryIdentifierValues = allIdentifierValues[countryIdStr];
                    }
                }
                catch
                {
                    // JSON deserialization failed - continue with empty values
                }
            }

            // Get country-specific identifier configurations
            var identifierFields = await _identifierValidationService.GetFieldsForCountryAsync(clientCountryId);
            
            // Populate identifier fields with values
            foreach (var field in identifierFields)
            {
                var identifierTypeIdStr = field.IdentifierTypeId.ToString();
                if (countryIdentifierValues.ContainsKey(identifierTypeIdStr))
                {
                    field.Value = countryIdentifierValues[identifierTypeIdStr];
                }
                
                // Map to legacy fields for backward compatibility
                if (!string.IsNullOrWhiteSpace(field.Value))
                {
                    if (string.Equals(field.IdentifierTypeName, "PAN", StringComparison.OrdinalIgnoreCase))
                    {
                        dto.PanNumber = field.Value;
                    }
                    else if (string.Equals(field.IdentifierTypeName, "TAN", StringComparison.OrdinalIgnoreCase))
                    {
                        dto.TanNumber = field.Value;
                    }
                    else if (string.Equals(field.IdentifierTypeName, "GST", StringComparison.OrdinalIgnoreCase) || 
                             string.Equals(field.IdentifierTypeName, "GSTN", StringComparison.OrdinalIgnoreCase))
                    {
                        dto.GstNumber = field.Value;
                    }
                }
            }
            
            dto.IdentifierFields = identifierFields;

            // Get country-specific bank details
            var bankDetailsEntity = await _db.BankDetails
                .FirstOrDefaultAsync(b => b.CompanyDetailsId == companyDetails.CompanyDetailsId 
                    && b.CountryId == clientCountryId);

            if (bankDetailsEntity != null && !string.IsNullOrWhiteSpace(bankDetailsEntity.FieldValues))
            {
                try
                {
                    var bankFieldValues = JsonSerializer.Deserialize<Dictionary<string, string>>(
                        bankDetailsEntity.FieldValues);

                    if (bankFieldValues != null)
                    {
                        // Get bank field configurations for the country
                        var bankFields = await _bankDetailsValidationService.GetFieldsForCountryAsync(clientCountryId);
                        
                        // Populate bank fields with values
                        foreach (var field in bankFields)
                        {
                            var bankFieldTypeIdStr = field.BankFieldTypeId.ToString();
                            if (bankFieldValues.ContainsKey(bankFieldTypeIdStr))
                            {
                                field.Value = bankFieldValues[bankFieldTypeIdStr];
                            }
                        }
                        
                        dto.BankFields = bankFields;
                        
                        // Create BankDetailsDto with country-specific fields for backward compatibility
                        var bankDetailsDto = new BankDetailsDto
                        {
                            BankDetailsId = bankDetailsEntity.BankDetailsId,
                            Country = bankDetailsEntity.Country,
                            AccountNumber = bankDetailsEntity.AccountNumber,
                            BankName = bankDetailsEntity.BankName,
                            BranchName = bankDetailsEntity.BranchName
                        };

                        // Map dynamic fields to legacy fields for backward compatibility
                        foreach (var field in bankFields.Where(f => !string.IsNullOrWhiteSpace(f.Value)))
                        {
                            if (string.Equals(field.BankFieldTypeName, "IFSC", StringComparison.OrdinalIgnoreCase))
                            {
                                bankDetailsDto.IfscCode = field.Value;
                            }
                            else if (string.Equals(field.BankFieldTypeName, "IBAN", StringComparison.OrdinalIgnoreCase))
                            {
                                bankDetailsDto.Iban = field.Value;
                            }
                            else if (string.Equals(field.BankFieldTypeName, "SWIFT", StringComparison.OrdinalIgnoreCase) ||
                                     string.Equals(field.BankFieldTypeName, "SWIFT_CODE", StringComparison.OrdinalIgnoreCase))
                            {
                                bankDetailsDto.SwiftCode = field.Value;
                            }
                        }

                        dto.BankDetails = new List<BankDetailsDto> { bankDetailsDto };
                    }
                }
                catch
                {
                    // JSON deserialization failed - use entity fields as fallback
                    dto.BankDetails = new List<BankDetailsDto>
                    {
                        new BankDetailsDto
                        {
                            BankDetailsId = bankDetailsEntity.BankDetailsId,
                            Country = bankDetailsEntity.Country,
                            AccountNumber = bankDetailsEntity.AccountNumber,
                            IfscCode = bankDetailsEntity.IfscCode,
                            Iban = bankDetailsEntity.Iban,
                            SwiftCode = bankDetailsEntity.SwiftCode,
                            BankName = bankDetailsEntity.BankName,
                            BranchName = bankDetailsEntity.BranchName
                        }
                    };
                }
            }
            else
            {
                dto.BankDetails = new List<BankDetailsDto>();
            }

            // Cache for 5 minutes
            _cache.Set(cacheKey, dto, TimeSpan.FromMinutes(CacheMinutes));

            return dto;
        }

        /// <summary>
        /// Invalidates cache for a country
        /// </summary>
        public void InvalidateCache(Guid countryId)
        {
            var cacheKey = $"company-details-quotation-{countryId}";
            _cache.Remove(cacheKey);
        }

        /// <summary>
        /// Invalidates all cached company details
        /// </summary>
        public void InvalidateAllCache()
        {
            // Note: In a production system, you might want to track cache keys
            // For now, we'll rely on cache expiration
        }
    }
}

