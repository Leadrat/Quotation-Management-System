using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.Quotations.Dtos;
using CRM.Application.Quotations.Services;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CRM.Shared.Config;

namespace CRM.Application.Quotations.Commands.Handlers
{
    public class CreateQuotationCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly QuotationNumberGenerator _numberGenerator;
        private readonly QuotationTotalsCalculator _totalsCalculator;
        private readonly TaxCalculationService _taxCalculator;
        private readonly QuotationSettings _settings;
        private readonly ILogger<CreateQuotationCommandHandler> _logger;

        public CreateQuotationCommandHandler(
            IAppDbContext db,
            IMapper mapper,
            QuotationNumberGenerator numberGenerator,
            QuotationTotalsCalculator totalsCalculator,
            TaxCalculationService taxCalculator,
            IOptions<QuotationSettings> settings,
            ILogger<CreateQuotationCommandHandler> logger)
        {
            _db = db;
            _mapper = mapper;
            _numberGenerator = numberGenerator;
            _totalsCalculator = totalsCalculator;
            _taxCalculator = taxCalculator;
            _settings = settings.Value;
            _logger = logger;
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

            // Calculate tax (handle null StateCode)
            var taxResult = _taxCalculator.CalculateTax(
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

            // Set quotation ID on line items
            foreach (var item in lineItems)
            {
                item.QuotationId = quotation.QuotationId;
            }

            quotation.LineItems = lineItems;

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
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                // Check if this is a missing table error (check both outer and inner exceptions)
                var exceptionMessage = ex.Message;
                var innerException = ex.InnerException;
                while (innerException != null)
                {
                    exceptionMessage += " | " + innerException.Message;
                    innerException = innerException.InnerException;
                }

                if (exceptionMessage.Contains("42P01") || 
                    exceptionMessage.Contains("does not exist") || 
                    (exceptionMessage.Contains("relation") && exceptionMessage.Contains("not exist")) ||
                    exceptionMessage.Contains("Invalid object name") ||
                    exceptionMessage.Contains("could not be found") ||
                    exceptionMessage.Contains("Quotations"))
                {
                    _logger.LogError(ex, "Quotations table does not exist, cannot create quotation for client {ClientId}", 
                        request.Request.ClientId);
                    throw new InvalidOperationException("Quotations table does not exist. Please run database migrations first.");
                }

                _logger.LogError(ex, "Failed to create quotation for client {ClientId} after {ElapsedMs}ms", 
                    request.Request.ClientId, stopwatch.ElapsedMilliseconds);
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

