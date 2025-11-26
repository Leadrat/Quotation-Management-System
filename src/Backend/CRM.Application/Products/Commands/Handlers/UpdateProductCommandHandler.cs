using System;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Products.DTOs;
using CRM.Application.Products.Services;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Products.Commands.Handlers
{
    public class UpdateProductCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IProductPricingService _pricingService;

        public UpdateProductCommandHandler(IAppDbContext db, IMapper mapper, IProductPricingService pricingService)
        {
            _db = db;
            _mapper = mapper;
            _pricingService = pricingService;
        }

        public async Task<ProductDto> Handle(UpdateProductCommand cmd)
        {
            var product = await _db.Products
                .FirstOrDefaultAsync(p => p.ProductId == cmd.ProductId);

            if (product == null)
            {
                throw new InvalidOperationException($"Product with ID {cmd.ProductId} not found.");
            }

            // Validate user exists
            var userExists = await _db.Users.AnyAsync(u => u.UserId == cmd.UpdatedByUserId && u.DeletedAt == null);
            if (!userExists)
            {
                throw new InvalidOperationException($"User with ID {cmd.UpdatedByUserId} does not exist.");
            }

            // Validate category if provided
            if (cmd.CategoryId.HasValue)
            {
                var categoryExists = await _db.ProductCategories.AnyAsync(c => c.CategoryId == cmd.CategoryId.Value && c.IsActive);
                if (!categoryExists)
                {
                    throw new InvalidOperationException($"Product category with ID {cmd.CategoryId.Value} does not exist or is inactive.");
                }
            }

            // JSONB fields should already be JSON strings from the API
            var billingCycleMultipliers = cmd.BillingCycleMultipliers;
            var addOnPricing = cmd.AddOnPricing;
            var customDevelopmentPricing = cmd.CustomDevelopmentPricing;

            // Track price changes for history
            var oldBasePrice = product.BasePricePerUserPerMonth;
            var oldAddOnPricing = product.AddOnPricing;
            var oldCustomDevPricing = product.CustomDevelopmentPricing;
            var oldMultipliers = product.BillingCycleMultipliers;

            // Update product properties
            product.ProductName = cmd.ProductName;
            product.ProductType = cmd.ProductType;
            product.Description = cmd.Description;
            product.CategoryId = cmd.CategoryId;
            product.BasePricePerUserPerMonth = cmd.BasePricePerUserPerMonth;
            product.BillingCycleMultipliers = billingCycleMultipliers;
            product.AddOnPricing = addOnPricing;
            product.CustomDevelopmentPricing = customDevelopmentPricing;
            product.Currency = cmd.Currency;
            product.IsActive = cmd.IsActive;
            product.UpdatedByUserId = cmd.UpdatedByUserId;
            product.UpdatedAt = DateTimeOffset.UtcNow;

            // Create price history entries for price changes
            var now = DateTime.UtcNow;
            if (oldBasePrice != cmd.BasePricePerUserPerMonth && cmd.BasePricePerUserPerMonth.HasValue)
            {
                var history = new ProductPriceHistory
                {
                    PriceHistoryId = Guid.NewGuid(),
                    ProductId = product.ProductId,
                    PriceType = PriceType.BasePrice,
                    OldPriceValue = oldBasePrice,
                    NewPriceValue = cmd.BasePricePerUserPerMonth.Value,
                    EffectiveFrom = now,
                    ChangedByUserId = cmd.UpdatedByUserId,
                    ChangedAt = DateTimeOffset.UtcNow
                };
                _db.ProductPriceHistory.Add(history);
            }

            // Update previous history entries to set EffectiveTo
            if (oldBasePrice != cmd.BasePricePerUserPerMonth || 
                oldAddOnPricing != addOnPricing || 
                oldCustomDevPricing != customDevelopmentPricing ||
                oldMultipliers != billingCycleMultipliers)
            {
                var previousEntries = await _db.ProductPriceHistory
                    .Where(h => h.ProductId == product.ProductId && h.EffectiveTo == null)
                    .ToListAsync();

                foreach (var entry in previousEntries)
                {
                    entry.EffectiveTo = now;
                }
            }

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                var innerException = dbEx.InnerException?.Message ?? dbEx.Message;
                throw new InvalidOperationException($"Database error while updating product: {innerException}", dbEx);
            }

            // Invalidate cache if pricing changed
            var pricingChanged = oldBasePrice != cmd.BasePricePerUserPerMonth ||
                                oldAddOnPricing != addOnPricing ||
                                oldCustomDevPricing != customDevelopmentPricing ||
                                oldMultipliers != billingCycleMultipliers;
            if (pricingChanged)
            {
                _pricingService.InvalidateProductCache(product.ProductId);
            }

            // Reload with navigation properties
            var updatedProduct = await _db.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == product.ProductId);

            if (updatedProduct == null)
            {
                throw new InvalidOperationException("Failed to retrieve updated product");
            }

            return _mapper.Map<ProductDto>(updatedProduct);
        }
    }
}

