using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.TaxManagement.Dtos;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Shared.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Application.TaxManagement.Services
{
    public class TaxCalculationService : ITaxCalculationService
    {
        private readonly IAppDbContext _db;
        private readonly IMemoryCache _cache;
        private readonly ILogger<TaxCalculationService> _logger;
        private readonly CompanySettings _companySettings;

        public TaxCalculationService(
            IAppDbContext db,
            IMemoryCache cache,
            ILogger<TaxCalculationService> logger,
            IOptions<CompanySettings> companySettings)
        {
            _db = db;
            _cache = cache;
            _logger = logger;
            _companySettings = companySettings.Value;
        }

        public async Task<TaxCalculationResultDto> CalculateTaxAsync(
            Guid clientId,
            IEnumerable<LineItemTaxInput> lineItems,
            decimal subtotal,
            decimal discountAmount,
            DateTime calculationDate,
            Guid? countryId = null,
            CancellationToken cancellationToken = default)
        {
            // Get client
            var client = await _db.Clients
                .FirstOrDefaultAsync(c => c.ClientId == clientId, cancellationToken);

            if (client == null)
            {
                throw new InvalidOperationException($"Client with ID '{clientId}' not found");
            }

            // Determine country and jurisdiction
            // Use provided countryId if available, otherwise use client's country
            if (!countryId.HasValue)
            {
                countryId = client.CountryId;
            }
            
            var jurisdictionId = client.JurisdictionId;

            // If still no country, use company default
            if (!countryId.HasValue)
            {
                var defaultCountry = await _db.Countries
                    .FirstOrDefaultAsync(c => c.IsDefault && c.IsActive && c.DeletedAt == null, cancellationToken);
                if (defaultCountry == null)
                {
                    throw new InvalidOperationException("No default country configured and client has no country assigned");
                }
                countryId = defaultCountry.CountryId;
            }

            // Get tax framework for country
            var taxFramework = await _db.TaxFrameworks
                .FirstOrDefaultAsync(f => f.CountryId == countryId.Value && f.IsActive && f.DeletedAt == null, cancellationToken);

            if (taxFramework == null)
            {
                throw new InvalidOperationException($"No active tax framework found for country ID '{countryId.Value}'");
            }

            var calculationDateOnly = DateOnly.FromDateTime(calculationDate);
            var taxableAmount = subtotal - discountAmount;

            // Load country and jurisdiction separately if needed
            string? countryName = null;
            string? jurisdictionName = null;
            
            if (countryId.HasValue)
            {
                var country = await _db.Countries
                    .FirstOrDefaultAsync(c => c.CountryId == countryId.Value, cancellationToken);
                countryName = country?.CountryName;
            }
            
            if (jurisdictionId.HasValue)
            {
                var jurisdiction = await _db.Jurisdictions
                    .FirstOrDefaultAsync(j => j.JurisdictionId == jurisdictionId.Value, cancellationToken);
                jurisdictionName = jurisdiction?.JurisdictionName;
            }

            var result = new TaxCalculationResultDto
            {
                Subtotal = subtotal,
                DiscountAmount = discountAmount,
                TaxableAmount = taxableAmount,
                CountryId = countryId,
                JurisdictionId = jurisdictionId,
                TaxFrameworkId = taxFramework.TaxFrameworkId,
                CountryName = countryName,
                JurisdictionName = jurisdictionName,
                FrameworkName = taxFramework.FrameworkName
            };

            // Determine if transaction is intra-state or inter-state (for GST)
            bool isIntraState = false;
            if (taxFramework.FrameworkType == TaxFrameworkType.GST) // GST framework
            {
                var clientStateCode = client.StateCode;
                var companyStateCode = _companySettings?.StateCode;
                
                if (!string.IsNullOrWhiteSpace(clientStateCode) && !string.IsNullOrWhiteSpace(companyStateCode))
                {
                    isIntraState = string.Equals(
                        clientStateCode.Trim(),
                        companyStateCode.Trim(),
                        StringComparison.OrdinalIgnoreCase);
                }
            }

            // Calculate tax for each line item
            var lineItemBreakdowns = new List<LineItemTaxBreakdownDto>();
            decimal totalTax = 0;

            foreach (var lineItem in lineItems)
            {
                var lineItemTax = await CalculateLineItemTaxAsync(
                    jurisdictionId,
                    taxFramework,
                    lineItem.ProductServiceCategoryId,
                    lineItem.Amount,
                    calculationDateOnly,
                    isIntraState,
                    cancellationToken);

                lineItemBreakdowns.Add(new LineItemTaxBreakdownDto
                {
                    LineItemId = lineItem.LineItemId,
                    CategoryId = lineItem.ProductServiceCategoryId,
                    Amount = lineItem.Amount,
                    TaxAmount = lineItemTax.TotalTax,
                    ComponentBreakdown = lineItemTax.ComponentBreakdown
                });

                totalTax += lineItemTax.TotalTax;
            }

            // Aggregate component breakdowns
            var aggregatedComponents = new Dictionary<string, TaxComponentBreakdownDto>();
            foreach (var lineItemBreakdown in lineItemBreakdowns)
            {
                foreach (var component in lineItemBreakdown.ComponentBreakdown)
                {
                    if (aggregatedComponents.ContainsKey(component.Component))
                    {
                        aggregatedComponents[component.Component].Amount += component.Amount;
                    }
                    else
                    {
                        aggregatedComponents[component.Component] = new TaxComponentBreakdownDto
                        {
                            Component = component.Component,
                            Rate = component.Rate,
                            Amount = component.Amount
                        };
                    }
                }
            }

            result.TaxBreakdown = aggregatedComponents.Values.ToList();
            result.TotalTax = totalTax;
            result.TotalAmount = taxableAmount + totalTax;
            result.LineItemBreakdown = lineItemBreakdowns;

            return result;
        }

        private async Task<LineItemTaxResult> CalculateLineItemTaxAsync(
            Guid? jurisdictionId,
            TaxFramework taxFramework,
            Guid? categoryId,
            decimal amount,
            DateOnly calculationDate,
            bool isIntraState,
            CancellationToken cancellationToken)
        {
            // Lookup tax rate using priority: Category+Jurisdiction -> Jurisdiction -> Country default
            TaxRate? taxRate = null;

            // 1. Try category-specific rate in jurisdiction
            if (jurisdictionId.HasValue && categoryId.HasValue)
            {
                taxRate = await GetTaxRateAsync(
                    jurisdictionId.Value,
                    taxFramework.TaxFrameworkId,
                    categoryId.Value,
                    calculationDate,
                    cancellationToken);
            }

            // 2. Try jurisdiction base rate
            if (taxRate == null && jurisdictionId.HasValue)
            {
                taxRate = await GetTaxRateAsync(
                    jurisdictionId.Value,
                    taxFramework.TaxFrameworkId,
                    null,
                    calculationDate,
                    cancellationToken);
            }

            // 3. Try parent jurisdiction rates (recursive)
            if (taxRate == null && jurisdictionId.HasValue)
            {
                taxRate = await GetTaxRateFromParentJurisdictionAsync(
                    jurisdictionId.Value,
                    taxFramework.TaxFrameworkId,
                    categoryId,
                    calculationDate,
                    cancellationToken);
            }

            // 4. Try country default rate with category (if category exists)
            if (taxRate == null && categoryId.HasValue)
            {
                taxRate = await GetTaxRateAsync(
                    null,
                    taxFramework.TaxFrameworkId,
                    categoryId,
                    calculationDate,
                    cancellationToken);
            }

            // 5. Try country default rate (general, no category)
            if (taxRate == null)
            {
                taxRate = await GetTaxRateAsync(
                    null,
                    taxFramework.TaxFrameworkId,
                    null,
                    calculationDate,
                    cancellationToken);
            }

            if (taxRate == null)
            {
                _logger.LogWarning("No tax rate found for jurisdiction {JurisdictionId}, framework {FrameworkId}, category {CategoryId}",
                    jurisdictionId, taxFramework.TaxFrameworkId, categoryId);
                return new LineItemTaxResult
                {
                    TotalTax = 0,
                    ComponentBreakdown = new List<TaxComponentBreakdownDto>()
                };
            }

            // Handle exempt and zero-rated
            if (taxRate.IsExempt || taxRate.IsZeroRated)
            {
                return new LineItemTaxResult
                {
                    TotalTax = 0,
                    ComponentBreakdown = new List<TaxComponentBreakdownDto>()
                };
            }

            // Calculate tax by component
            var componentRates = taxRate.GetTaxComponentRates();
            var componentBreakdown = new List<TaxComponentBreakdownDto>();

            // For GST framework, determine which components to use based on intra-state vs inter-state
            if (taxFramework.FrameworkType == TaxFrameworkType.GST) // GST
            {
                if (isIntraState)
                {
                    // Intra-state: Use CGST and SGST (split the total rate)
                    var totalRate = componentRates.Sum(c => c.Rate);
                    var cgstRate = totalRate / 2m;
                    var sgstRate = totalRate / 2m;
                    
                    componentBreakdown.Add(new TaxComponentBreakdownDto
                    {
                        Component = "CGST",
                        Rate = cgstRate,
                        Amount = amount * (cgstRate / 100m)
                    });
                    
                    componentBreakdown.Add(new TaxComponentBreakdownDto
                    {
                        Component = "SGST",
                        Rate = sgstRate,
                        Amount = amount * (sgstRate / 100m)
                    });
                }
                else
                {
                    // Inter-state: Use IGST (total rate)
                    var totalRate = componentRates.Sum(c => c.Rate);
                    
                    componentBreakdown.Add(new TaxComponentBreakdownDto
                    {
                        Component = "IGST",
                        Rate = totalRate,
                        Amount = amount * (totalRate / 100m)
                    });
                }
            }
            else
            {
                // For non-GST frameworks (VAT, etc.), use components as-is
                foreach (var componentRate in componentRates)
                {
                    var componentAmount = amount * (componentRate.Rate / 100m);
                    componentBreakdown.Add(new TaxComponentBreakdownDto
                    {
                        Component = componentRate.Component,
                        Rate = componentRate.Rate,
                        Amount = componentAmount
                    });
                }
            }

            var totalTax = componentBreakdown.Sum(c => c.Amount);

            return new LineItemTaxResult
            {
                TotalTax = totalTax,
                ComponentBreakdown = componentBreakdown
            };
        }

        private async Task<TaxRate?> GetTaxRateAsync(
            Guid? jurisdictionId,
            Guid taxFrameworkId,
            Guid? categoryId,
            DateOnly calculationDate,
            CancellationToken cancellationToken)
        {
            var cacheKey = $"TaxRate_{jurisdictionId}_{taxFrameworkId}_{categoryId}_{calculationDate:yyyy-MM-dd}";

            if (_cache.TryGetValue(cacheKey, out TaxRate? cachedRate))
            {
                return cachedRate;
            }

            // Build query with proper null handling for Entity Framework
            var query = _db.TaxRates
                .Where(tr => tr.TaxFrameworkId == taxFrameworkId &&
                    tr.EffectiveFrom <= calculationDate &&
                    (tr.EffectiveTo == null || tr.EffectiveTo >= calculationDate));

            // Handle null comparisons properly for Entity Framework
            if (jurisdictionId.HasValue)
            {
                query = query.Where(tr => tr.JurisdictionId == jurisdictionId.Value);
            }
            else
            {
                query = query.Where(tr => tr.JurisdictionId == null);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(tr => tr.ProductServiceCategoryId == categoryId.Value);
            }
            else
            {
                query = query.Where(tr => tr.ProductServiceCategoryId == null);
            }

            var rate = await query
                .OrderByDescending(tr => tr.EffectiveFrom)
                .FirstOrDefaultAsync(cancellationToken);

            if (rate != null)
            {
                _cache.Set(cacheKey, rate, TimeSpan.FromHours(1));
            }

            return rate;
        }

        private async Task<TaxRate?> GetTaxRateFromParentJurisdictionAsync(
            Guid jurisdictionId,
            Guid taxFrameworkId,
            Guid? categoryId,
            DateOnly calculationDate,
            CancellationToken cancellationToken)
        {
            var jurisdiction = await _db.Jurisdictions
                .FirstOrDefaultAsync(j => j.JurisdictionId == jurisdictionId && j.DeletedAt == null, cancellationToken);

            if (jurisdiction?.ParentJurisdictionId == null)
            {
                return null;
            }

            // Try parent jurisdiction
            return await GetTaxRateAsync(
                jurisdiction.ParentJurisdictionId,
                taxFrameworkId,
                categoryId,
                calculationDate,
                cancellationToken);
        }

        private class LineItemTaxResult
        {
            public decimal TotalTax { get; set; }
            public List<TaxComponentBreakdownDto> ComponentBreakdown { get; set; } = new();
        }
    }
}

