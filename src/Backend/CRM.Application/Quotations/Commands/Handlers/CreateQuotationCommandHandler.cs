using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.Quotations.Dtos;
using CRM.Application.Quotations.Services;
using CRM.Application.TaxManagement.Services;
using CRM.Application.TaxManagement.Dtos;
using CRM.Application.CompanyDetails.Services;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CRM.Shared.Config;
using System.Text.Json;

namespace CRM.Application.Quotations.Commands.Handlers
{
    public class CreateQuotationCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly QuotationNumberGenerator _numberGenerator;
        private readonly QuotationTotalsCalculator _totalsCalculator;
        private readonly Quotations.Services.TaxCalculationService _taxCalculator; // Legacy service for backward compatibility
        private readonly ITaxCalculationService _newTaxCalculator; // New framework-based service
        private readonly QuotationSettings _settings;
        private readonly ILogger<CreateQuotationCommandHandler> _logger;
        private readonly ICompanyDetailsService _companyDetailsService;
        private readonly Quotations.Services.QuotationCompanyDetailsService _quotationCompanyDetailsService;

        public CreateQuotationCommandHandler(
            IAppDbContext db,
            IMapper mapper,
            QuotationNumberGenerator numberGenerator,
            QuotationTotalsCalculator totalsCalculator,
            Quotations.Services.TaxCalculationService taxCalculator,
            ITaxCalculationService newTaxCalculator,
            IOptions<QuotationSettings> settings,
            ILogger<CreateQuotationCommandHandler> logger,
            ICompanyDetailsService companyDetailsService,
            Quotations.Services.QuotationCompanyDetailsService quotationCompanyDetailsService)
        {
            _db = db;
            _mapper = mapper;
            _numberGenerator = numberGenerator;
            _totalsCalculator = totalsCalculator;
            _taxCalculator = taxCalculator;
            _newTaxCalculator = newTaxCalculator;
            _settings = settings.Value;
            _logger = logger;
            _companyDetailsService = companyDetailsService;
            _quotationCompanyDetailsService = quotationCompanyDetailsService;
        }

        public async Task<QuotationDto> Handle(CreateQuotationCommand request)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Creating quotation for client {ClientId} by user {UserId}", 
                request.Request.ClientId, request.CreatedByUserId);

            try
            {
                // Validate client exists and user has access
                var client = await _db.Clients
                    .FirstOrDefaultAsync(c => c.ClientId == request.Request.ClientId);

            if (client == null)
            {
                throw new InvalidOperationException($"Client with ID {request.Request.ClientId} not found.");
            }

            // Check ownership (user created the client or is admin)
            var isAdmin = await IsAdminAsync(request.CreatedByUserId);
            if (!isAdmin && client.CreatedByUserId != request.CreatedByUserId)
            {
                throw new UnauthorizedAccessException("You do not have permission to create quotations for this client.");
            }

            // Generate quotation number
            var quotationNumber = await _numberGenerator.GenerateAsync();

            // Set dates - ensure UTC for PostgreSQL (PostgreSQL requires UTC for timestamp with time zone)
            DateTime quotationDate;
            if (request.Request.QuotationDate.HasValue)
            {
                var date = request.Request.QuotationDate.Value;
                // Convert to UTC if not already UTC
                quotationDate = date.Kind == DateTimeKind.Utc 
                    ? date 
                    : date.Kind == DateTimeKind.Local 
                        ? date.ToUniversalTime() 
                        : DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            }
            else
            {
                // Default to today at midnight UTC
                quotationDate = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);
            }
            
            DateTime validUntil;
            if (request.Request.ValidUntil.HasValue)
            {
                var date = request.Request.ValidUntil.Value;
                // Convert to UTC if not already UTC
                validUntil = date.Kind == DateTimeKind.Utc 
                    ? date 
                    : date.Kind == DateTimeKind.Local 
                        ? date.ToUniversalTime() 
                        : DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            }
            else
            {
                // Default to quotation date + default valid days at midnight UTC
                validUntil = DateTime.SpecifyKind(quotationDate.Date.AddDays(_settings.DefaultValidDays), DateTimeKind.Utc);
            }

            // Create line items
            var lineItems = new List<QuotationLineItem>();
            for (int i = 0; i < request.Request.LineItems.Count; i++)
            {
                var lineItemRequest = request.Request.LineItems[i];
                var lineItem = _mapper.Map<QuotationLineItem>(lineItemRequest);
                lineItem.LineItemId = Guid.NewGuid();
                lineItem.QuotationId = Guid.NewGuid(); // Will be set after quotation is created
                lineItem.SequenceNumber = i + 1;
                lineItem.CalculateAmount();
                lineItem.CreatedAt = DateTimeOffset.UtcNow;
                lineItem.UpdatedAt = DateTimeOffset.UtcNow;
                lineItems.Add(lineItem);
            }

            // Calculate totals
            var totals = _totalsCalculator.Calculate(null!, lineItems, request.Request.DiscountPercentage);

            // Calculate tax using new framework-based service
            TaxCalculationResultDto? newTaxResult = null;
            try
            {
                var lineItemTaxInputs = lineItems.Select(li => new LineItemTaxInput
                {
                    LineItemId = li.LineItemId,
                    // Use TaxCategoryId if ProductServiceCategoryId is not set (for backward compatibility)
                    ProductServiceCategoryId = li.ProductServiceCategoryId ?? li.TaxCategoryId,
                    Amount = li.Amount
                }).ToList();

                newTaxResult = await _newTaxCalculator.CalculateTaxAsync(
                    request.Request.ClientId,
                    lineItemTaxInputs,
                    totals.SubTotal,
                    totals.DiscountAmount,
                    quotationDate);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to calculate tax using new service, falling back to legacy service");
            }

            // Fallback to legacy tax calculation if new service fails
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
                    client.StateCode ?? null);

            // Create quotation entity
            var quotation = new Quotation
            {
                QuotationId = Guid.NewGuid(),
                ClientId = request.Request.ClientId,
                CreatedByUserId = request.CreatedByUserId,
                QuotationNumber = quotationNumber,
                Status = QuotationStatus.Draft,
                QuotationDate = quotationDate,
                ValidUntil = validUntil,
                SubTotal = totals.SubTotal,
                DiscountAmount = totals.DiscountAmount,
                DiscountPercentage = request.Request.DiscountPercentage,
                TaxAmount = taxResult.TotalTax,
                CgstAmount = taxResult.CgstAmount,
                SgstAmount = taxResult.SgstAmount,
                IgstAmount = taxResult.IgstAmount,
                TotalAmount = totals.SubTotal - totals.DiscountAmount + taxResult.TotalTax,
                Notes = request.Request.Notes,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            // Set new tax framework fields if new tax calculation was used
            if (newTaxResult != null)
            {
                quotation.TaxCountryId = newTaxResult.CountryId;
                quotation.TaxJurisdictionId = newTaxResult.JurisdictionId;
                quotation.TaxFrameworkId = newTaxResult.TaxFrameworkId;
                quotation.TaxBreakdown = JsonSerializer.Serialize(newTaxResult.TaxBreakdown);

                // Log tax calculation
                var taxLog = new TaxCalculationLog
                {
                    LogId = Guid.NewGuid(),
                    QuotationId = quotation.QuotationId,
                    ActionType = TaxCalculationActionType.Calculation,
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
                    ChangedByUserId = request.CreatedByUserId,
                    ChangedAt = DateTimeOffset.UtcNow
                };
                _db.TaxCalculationLogs.Add(taxLog);
            }

            // Set quotation ID on line items
            foreach (var item in lineItems)
            {
                item.QuotationId = quotation.QuotationId;
            }

            quotation.LineItems = lineItems;

            // Store company details snapshot for historical accuracy (country-specific)
            try
            {
                var clientCountryId = client.CountryId ?? throw new InvalidOperationException("Client must have a country set.");
                var companyDetails = await _quotationCompanyDetailsService.GetCompanyDetailsForQuotationAsync(clientCountryId);
                if (companyDetails != null)
                {
                    quotation.CompanyDetailsSnapshot = JsonSerializer.Serialize(companyDetails);
                    _logger.LogInformation("Stored country-specific company details snapshot for quotation {QuotationId}, client country: {CountryId}", 
                        quotation.QuotationId, clientCountryId);
                }
                else
                {
                    _logger.LogWarning("Company details not configured for country {CountryId}. Quotation {QuotationId} created without company details snapshot.", 
                        clientCountryId, quotation.QuotationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get country-specific company details for quotation {QuotationId}. Falling back to general company details.", 
                    quotation.QuotationId);
                // Fallback to general company details
                var companyDetails = await _companyDetailsService.GetCompanyDetailsAsync();
                if (companyDetails != null)
                {
                    quotation.CompanyDetailsSnapshot = JsonSerializer.Serialize(companyDetails);
                }
            }

            // Save to database
            _db.Quotations.Add(quotation);
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                var innerMessage = dbEx.InnerException?.Message ?? "";
                var fullMessage = $"{dbEx.Message} | {innerMessage}";
                
                _logger.LogError(dbEx, "Database error saving quotation for client {ClientId}. Error: {Error}", 
                    request.Request.ClientId, fullMessage);
                
                // Check for specific constraint violations
                if (fullMessage.Contains("foreign key") || fullMessage.Contains("violates foreign key constraint"))
                {
                    if (fullMessage.Contains("ClientId") || fullMessage.Contains("Clients"))
                    {
                        throw new InvalidOperationException($"Client with ID {request.Request.ClientId} does not exist in the database.", dbEx);
                    }
                    if (fullMessage.Contains("CreatedByUserId") || fullMessage.Contains("Users"))
                    {
                        throw new InvalidOperationException($"User with ID {request.CreatedByUserId} does not exist in the database.", dbEx);
                    }
                }
                
                if (fullMessage.Contains("unique constraint") || fullMessage.Contains("duplicate key") || fullMessage.Contains("QuotationNumber"))
                {
                    throw new InvalidOperationException($"A quotation with number {quotationNumber} already exists. Please try again.", dbEx);
                }
                
                if (fullMessage.Contains("check constraint") || fullMessage.Contains("CK_"))
                {
                    if (fullMessage.Contains("ValidUntil") || fullMessage.Contains("QuotationDate"))
                    {
                        throw new InvalidOperationException($"Valid Until date must be after Quotation Date.", dbEx);
                    }
                    if (fullMessage.Contains("DiscountPercentage"))
                    {
                        throw new InvalidOperationException($"Discount percentage must be between 0 and 100.", dbEx);
                    }
                }
                
                // Check for missing table error (PostgreSQL error code 42P01)
                if (fullMessage.Contains("42P01") && 
                    (fullMessage.Contains("does not exist") || 
                     (fullMessage.Contains("relation") && fullMessage.Contains("not exist"))))
                {
                    _logger.LogError(dbEx, "Database table missing error. Error: {Error}", fullMessage);
                    throw new InvalidOperationException("Database table missing. Please contact administrator to run migrations.", dbEx);
                }
                
                throw new InvalidOperationException($"Failed to save quotation: {innerMessage}", dbEx);
            }

            // Load with navigation properties for mapping
            var savedQuotation = await _db.Quotations
                .Include(q => q.Client)
                .Include(q => q.CreatedByUser)
                .Include(q => q.LineItems)
                .FirstOrDefaultAsync(q => q.QuotationId == quotation.QuotationId);

            // Publish domain event
            var domainEvent = new QuotationCreated
            {
                QuotationId = quotation.QuotationId,
                QuotationNumber = quotation.QuotationNumber,
                ClientId = quotation.ClientId,
                CreatedByUserId = quotation.CreatedByUserId,
                TotalAmount = quotation.TotalAmount,
                CreatedAt = quotation.CreatedAt
            };

            // Map to DTO
            var result = _mapper.Map<QuotationDto>(savedQuotation);
            
            stopwatch.Stop();
            _logger.LogInformation("Quotation {QuotationId} created successfully in {ElapsedMs}ms. Total: {TotalAmount}, Tax: {TaxAmount}", 
                result.QuotationId, stopwatch.ElapsedMilliseconds, result.TotalAmount, result.TaxAmount);
            
            return result;
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                stopwatch.Stop();
                
                // This should already be handled by the inner catch block
                // Re-throw to preserve the specific error message
                throw;
            }
            catch (InvalidOperationException ioEx)
            {
                stopwatch.Stop();
                // Check if this is the "table does not exist" error we're looking for
                if (ioEx.Message.Contains("table does not exist") || ioEx.Message.Contains("run database migrations"))
                {
                    _logger.LogError(ioEx, "Database table missing error for quotation creation. Client: {ClientId}", 
                        request.Request.ClientId);
                }
                throw;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Failed to create quotation for client {ClientId} after {ElapsedMs}ms. Error: {Error}", 
                    request.Request.ClientId, stopwatch.ElapsedMilliseconds, ex.Message);
                throw;
            }
        }

        private async Task<bool> IsAdminAsync(Guid userId)
        {
            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null || user.Role == null) return false;

            return string.Equals(user.Role.RoleName, "Admin", StringComparison.OrdinalIgnoreCase);
        }
    }
}

