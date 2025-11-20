using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.Quotations.Dtos;
using CRM.Application.Quotations.Exceptions;
using CRM.Application.Quotations.Services;
using CRM.Application.TaxManagement.Services;
using CRM.Application.TaxManagement.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using CRM.Shared.Config;
using System.Text.Json;
using System.Linq;

namespace CRM.Application.Quotations.Commands.Handlers
{
    public class UpdateQuotationCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly QuotationTotalsCalculator _totalsCalculator;
        private readonly Quotations.Services.TaxCalculationService _taxCalculator; // Legacy service
        private readonly ITaxCalculationService _newTaxCalculator; // New framework-based service
        private readonly QuotationSettings _settings;
        private readonly Quotations.Services.QuotationCompanyDetailsService _quotationCompanyDetailsService;

        public UpdateQuotationCommandHandler(
            IAppDbContext db,
            IMapper mapper,
            QuotationTotalsCalculator totalsCalculator,
            Quotations.Services.TaxCalculationService taxCalculator,
            ITaxCalculationService newTaxCalculator,
            IOptions<QuotationSettings> settings,
            Quotations.Services.QuotationCompanyDetailsService quotationCompanyDetailsService)
        {
            _db = db;
            _mapper = mapper;
            _totalsCalculator = totalsCalculator;
            _taxCalculator = taxCalculator;
            _newTaxCalculator = newTaxCalculator;
            _settings = settings.Value;
            _quotationCompanyDetailsService = quotationCompanyDetailsService;
        }

        public async Task<QuotationDto> Handle(UpdateQuotationCommand command)
        {
            var quotation = await _db.Quotations
                .Include(q => q.Client)
                .Include(q => q.LineItems)
                .FirstOrDefaultAsync(q => q.QuotationId == command.QuotationId);

            if (quotation == null)
            {
                throw new QuotationNotFoundException(command.QuotationId);
            }

            // Authorization: User owns quotation or is admin
            var isAdmin = string.Equals(command.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin && quotation.CreatedByUserId != command.UpdatedByUserId)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this quotation.");
            }

            // Only draft quotations can be updated
            if (quotation.Status != Domain.Enums.QuotationStatus.Draft)
            {
                throw new InvalidQuotationStatusException("Only draft quotations can be updated.");
            }

            // Update quotation fields
            if (command.Request.QuotationDate.HasValue)
            {
                quotation.QuotationDate = command.Request.QuotationDate.Value;
            }

            if (command.Request.ValidUntil.HasValue)
            {
                quotation.ValidUntil = command.Request.ValidUntil.Value;
            }

            if (command.Request.DiscountPercentage.HasValue)
            {
                quotation.DiscountPercentage = command.Request.DiscountPercentage.Value;
            }

            if (command.Request.Notes != null)
            {
                quotation.Notes = command.Request.Notes;
            }

            // Update line items if provided
            if (command.Request.LineItems != null && command.Request.LineItems.Any())
            {
                // Remove existing line items
                _db.QuotationLineItems.RemoveRange(quotation.LineItems);

                // Add new/updated line items
                var newLineItems = new System.Collections.Generic.List<Domain.Entities.QuotationLineItem>();
                for (int i = 0; i < command.Request.LineItems.Count; i++)
                {
                    var lineItemRequest = command.Request.LineItems[i];
                    Domain.Entities.QuotationLineItem lineItem;

                    if (lineItemRequest.LineItemId.HasValue)
                    {
                        // Update existing line item
                        lineItem = quotation.LineItems.FirstOrDefault(li => li.LineItemId == lineItemRequest.LineItemId.Value);
                        if (lineItem != null)
                        {
                            _mapper.Map(lineItemRequest, lineItem);
                            lineItem.SequenceNumber = i + 1;
                            lineItem.CalculateAmount();
                            lineItem.UpdatedAt = DateTimeOffset.UtcNow;
                        }
                        else
                        {
                            // Line item not found, create new
                            lineItem = _mapper.Map<Domain.Entities.QuotationLineItem>(lineItemRequest);
                            lineItem.LineItemId = Guid.NewGuid();
                            lineItem.QuotationId = quotation.QuotationId;
                            lineItem.SequenceNumber = i + 1;
                            lineItem.CreatedAt = DateTimeOffset.UtcNow;
                            lineItem.UpdatedAt = DateTimeOffset.UtcNow;
                            newLineItems.Add(lineItem);
                        }
                    }
                    else
                    {
                        // New line item
                        lineItem = _mapper.Map<Domain.Entities.QuotationLineItem>(lineItemRequest);
                        lineItem.LineItemId = Guid.NewGuid();
                        lineItem.QuotationId = quotation.QuotationId;
                        lineItem.SequenceNumber = i + 1;
                        lineItem.CreatedAt = DateTimeOffset.UtcNow;
                        lineItem.UpdatedAt = DateTimeOffset.UtcNow;
                        newLineItems.Add(lineItem);
                    }
                }

                if (newLineItems.Any())
                {
                    _db.QuotationLineItems.AddRange(newLineItems);
                }

                // Reload line items for calculation
                await _db.SaveChangesAsync();
                quotation = await _db.Quotations
                    .Include(q => q.LineItems)
                    .FirstOrDefaultAsync(q => q.QuotationId == quotation.QuotationId);
            }

            // Recalculate totals
            var totals = _totalsCalculator.Calculate(quotation!, quotation.LineItems.ToList(), quotation.DiscountPercentage);

            // Recalculate tax using new framework-based service
            TaxCalculationResultDto? newTaxResult = null;
            try
            {
                var lineItemTaxInputs = quotation.LineItems.Select(li => new LineItemTaxInput
                {
                    LineItemId = li.LineItemId,
                    // Use TaxCategoryId if ProductServiceCategoryId is not set (for backward compatibility)
                    ProductServiceCategoryId = li.ProductServiceCategoryId ?? li.TaxCategoryId,
                    Amount = li.Amount
                }).ToList();

                newTaxResult = await _newTaxCalculator.CalculateTaxAsync(
                    quotation.ClientId,
                    lineItemTaxInputs,
                    totals.SubTotal,
                    totals.DiscountAmount,
                    quotation.QuotationDate);
            }
            catch (Exception)
            {
                // Fallback to legacy service
            }

            // Fallback to legacy tax calculation if new service fails
            var clientStateCode = quotation.Client?.StateCode;
            var taxResult = newTaxResult != null
                ? new TaxCalculationResult
                {
                    TotalTax = newTaxResult.TotalTax,
                    CgstAmount = newTaxResult.TaxBreakdown.FirstOrDefault(t => t.Component == "CGST")?.Amount ?? 0,
                    SgstAmount = newTaxResult.TaxBreakdown.FirstOrDefault(t => t.Component == "SGST")?.Amount ?? 0,
                    IgstAmount = newTaxResult.TaxBreakdown.FirstOrDefault(t => t.Component == "IGST")?.Amount ?? 0
                }
                : _taxCalculator.CalculateTax(
                    totals.SubTotal,
                    totals.DiscountAmount,
                    clientStateCode);

            // Update quotation totals
            quotation.SubTotal = totals.SubTotal;
            quotation.DiscountAmount = totals.DiscountAmount;
            quotation.TaxAmount = taxResult.TotalTax;
            quotation.CgstAmount = taxResult.CgstAmount;
            quotation.SgstAmount = taxResult.SgstAmount;
            quotation.IgstAmount = taxResult.IgstAmount;
            quotation.TotalAmount = totals.SubTotal - totals.DiscountAmount + taxResult.TotalTax;
            quotation.UpdatedAt = DateTimeOffset.UtcNow;

            // Update company details snapshot if client country changed
            if (quotation.Client != null && quotation.Client.CountryId.HasValue)
            {
                try
                {
                    var clientCountryId = quotation.Client.CountryId.Value;
                    var companyDetails = await _quotationCompanyDetailsService.GetCompanyDetailsForQuotationAsync(clientCountryId);
                    if (companyDetails != null)
                    {
                        quotation.CompanyDetailsSnapshot = JsonSerializer.Serialize(companyDetails);
                    }
                }
                catch
                {
                    // Ignore errors - keep existing snapshot
                }
            }

            // Set new tax framework fields if new tax calculation was used
            if (newTaxResult != null)
            {
                quotation.TaxCountryId = newTaxResult.CountryId;
                quotation.TaxJurisdictionId = newTaxResult.JurisdictionId;
                quotation.TaxFrameworkId = newTaxResult.TaxFrameworkId;
                quotation.TaxBreakdown = JsonSerializer.Serialize(newTaxResult.TaxBreakdown);

                // Log tax recalculation
                var taxLog = new Domain.Entities.TaxCalculationLog
                {
                    LogId = Guid.NewGuid(),
                    QuotationId = quotation.QuotationId,
                    ActionType = Domain.Enums.TaxCalculationActionType.Calculation,
                    CountryId = newTaxResult.CountryId,
                    JurisdictionId = newTaxResult.JurisdictionId,
                    CalculationDetails = JsonSerializer.Serialize(new
                    {
                        Subtotal = newTaxResult.Subtotal,
                        DiscountAmount = newTaxResult.DiscountAmount,
                        TaxableAmount = newTaxResult.TaxableAmount,
                        TotalTax = newTaxResult.TotalTax,
                        TaxBreakdown = newTaxResult.TaxBreakdown,
                        LineItemBreakdown = newTaxResult.LineItemBreakdown
                    }),
                    ChangedByUserId = command.UpdatedByUserId,
                    ChangedAt = DateTimeOffset.UtcNow
                };
                _db.TaxCalculationLogs.Add(taxLog);
            }

            await _db.SaveChangesAsync();

            // Load with navigation properties for mapping
            var updatedQuotation = await _db.Quotations
                .Include(q => q.Client)
                .Include(q => q.CreatedByUser)
                .Include(q => q.LineItems)
                .FirstOrDefaultAsync(q => q.QuotationId == quotation.QuotationId);

            return _mapper.Map<QuotationDto>(updatedQuotation);
        }
    }
}

