using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Products.Commands;
using CRM.Application.Products.Services;
using CRM.Application.Quotations.Dtos;
using CRM.Application.Localization.Services;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Products.Commands.Handlers
{
    public class AddProductToQuotationCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IProductPricingService _pricingService;
        private readonly ICurrencyService _currencyService;
        private readonly ILogger<AddProductToQuotationCommandHandler> _logger;

        public AddProductToQuotationCommandHandler(
            IAppDbContext db, 
            IMapper mapper, 
            IProductPricingService pricingService,
            ICurrencyService currencyService,
            ILogger<AddProductToQuotationCommandHandler> logger)
        {
            _db = db;
            _mapper = mapper;
            _pricingService = pricingService;
            _currencyService = currencyService;
            _logger = logger;
        }

        public async Task<LineItemDto> Handle(AddProductToQuotationCommand cmd)
        {
            // Validate quotation exists and is in draft status
            var quotation = await _db.Quotations
                .Include(q => q.LineItems)
                .Include(q => q.Client)
                .FirstOrDefaultAsync(q => q.QuotationId == cmd.QuotationId);

            if (quotation == null)
            {
                throw new InvalidOperationException($"Quotation with ID {cmd.QuotationId} not found.");
            }

            if (quotation.Status != Domain.Enums.QuotationStatus.Draft)
            {
                throw new InvalidOperationException("Products can only be added to draft quotations.");
            }

            // Get product
            var product = await _db.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == cmd.ProductId && p.IsActive);

            if (product == null)
            {
                throw new InvalidOperationException($"Product with ID {cmd.ProductId} not found or is inactive.");
            }

            // Calculate price based on product type
            decimal calculatedPrice = 0;
            decimal? originalProductPrice = null;

            if (product.ProductType == ProductType.Subscription)
            {
                var billingCycle = cmd.BillingCycle ?? BillingCycle.Monthly;
                calculatedPrice = _pricingService.CalculatePrice(product, (int)cmd.Quantity, billingCycle);
                originalProductPrice = product.BasePricePerUserPerMonth;
            }
            else if (product.ProductType == ProductType.AddOnSubscription || product.ProductType == ProductType.AddOnOneTime)
            {
                calculatedPrice = _pricingService.CalculatePrice(product, (int)cmd.Quantity);
            }
            else if (product.ProductType == ProductType.CustomDevelopment)
            {
                // Check if pricing model is hourly and hours are required
                if (!string.IsNullOrWhiteSpace(product.CustomDevelopmentPricing))
                {
                    try
                    {
                        var pricing = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(
                            product.CustomDevelopmentPricing,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (pricing != null && pricing.ContainsKey("pricingModel") && 
                            pricing["pricingModel"]?.ToString()?.ToLower() == "hourly" && !cmd.Hours.HasValue)
                        {
                            throw new InvalidOperationException("Hours are required for hourly custom development products.");
                        }
                    }
                    catch
                    {
                        // If parsing fails, continue with calculation
                    }
                }
                calculatedPrice = _pricingService.CalculatePrice(product, (int)cmd.Quantity, null, cmd.Hours);
            }

            // Handle currency conversion if product currency differs from client/quotation currency
            // For now, use product currency as-is. Currency conversion can be added when quotation currency field is available
            // TODO: Add Currency field to Quotation entity (Spec-017 integration) for full multi-currency support
            // For now, products maintain their own currency and conversion happens at display time if needed

            // Calculate unit rate
            var unitRate = cmd.Quantity > 0 ? calculatedPrice / cmd.Quantity : calculatedPrice;

            // Get next sequence number
            var maxSequence = quotation.LineItems.Any() ? quotation.LineItems.Max(li => li.SequenceNumber) : 0;

            // Create line item
            // Map TaxCategoryId to ProductServiceCategoryId for tax calculation
            var taxCategoryId = cmd.TaxCategoryId ?? product.CategoryId;
            var lineItem = new QuotationLineItem
            {
                LineItemId = Guid.NewGuid(),
                QuotationId = cmd.QuotationId,
                SequenceNumber = maxSequence + 1,
                ItemName = product.ProductName,
                Description = product.Description,
                Quantity = cmd.Quantity,
                UnitRate = unitRate,
                ProductId = product.ProductId,
                BillingCycle = cmd.BillingCycle,
                Hours = cmd.Hours,
                OriginalProductPrice = originalProductPrice,
                TaxCategoryId = taxCategoryId,
                ProductServiceCategoryId = taxCategoryId, // Map to ProductServiceCategoryId for tax calculation
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            lineItem.CalculateAmount();

            _db.QuotationLineItems.Add(lineItem);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                var innerException = dbEx.InnerException?.Message ?? dbEx.Message;
                throw new InvalidOperationException($"Database error while adding product to quotation: {innerException}", dbEx);
            }

            // Reload with navigation properties
            var lineItemWithNav = await _db.QuotationLineItems
                .Include(li => li.Product)
                .FirstOrDefaultAsync(li => li.LineItemId == lineItem.LineItemId);

            if (lineItemWithNav == null)
            {
                throw new InvalidOperationException("Failed to retrieve created line item");
            }

            return _mapper.Map<LineItemDto>(lineItemWithNav);
        }
    }
}

