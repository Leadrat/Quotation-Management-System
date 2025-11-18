using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.Quotations.Dtos;
using CRM.Application.Quotations.Exceptions;
using CRM.Application.Quotations.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using CRM.Shared.Config;

namespace CRM.Application.Quotations.Commands.Handlers
{
    public class UpdateQuotationCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly QuotationTotalsCalculator _totalsCalculator;
        private readonly TaxCalculationService _taxCalculator;
        private readonly QuotationSettings _settings;

        public UpdateQuotationCommandHandler(
            IAppDbContext db,
            IMapper mapper,
            QuotationTotalsCalculator totalsCalculator,
            TaxCalculationService taxCalculator,
            IOptions<QuotationSettings> settings)
        {
            _db = db;
            _mapper = mapper;
            _totalsCalculator = totalsCalculator;
            _taxCalculator = taxCalculator;
            _settings = settings.Value;
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

            // Recalculate tax
            var clientStateCode = quotation.Client?.StateCode;
            var taxResult = _taxCalculator.CalculateTax(
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

