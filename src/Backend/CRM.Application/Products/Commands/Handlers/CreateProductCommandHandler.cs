using System;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Products.DTOs;
using CRM.Application.Products.Services;
using CRM.Domain.Entities;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Products.Commands.Handlers
{
    public class CreateProductCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IProductPricingService _pricingService;

        public CreateProductCommandHandler(IAppDbContext db, IMapper mapper, IProductPricingService pricingService)
        {
            _db = db;
            _mapper = mapper;
            _pricingService = pricingService;
        }

        public async Task<ProductDto> Handle(CreateProductCommand cmd)
        {
            // Validate that the user exists
            var userExists = await _db.Users.AnyAsync(u => u.UserId == cmd.CreatedByUserId && u.DeletedAt == null);
            if (!userExists)
            {
                throw new InvalidOperationException($"User with ID {cmd.CreatedByUserId} does not exist.");
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

            var now = DateTimeOffset.UtcNow;
            
            // Serialize JSONB fields if they're objects (from frontend)
            var billingCycleMultipliers = cmd.BillingCycleMultipliers;
            if (billingCycleMultipliers != null && !billingCycleMultipliers.TrimStart().StartsWith("{"))
            {
                // If it's not already JSON, try to serialize it
                billingCycleMultipliers = _pricingService.SerializeBillingCycleMultipliers(cmd.BillingCycleMultipliers);
            }

            var addOnPricing = cmd.AddOnPricing;
            if (addOnPricing != null && !addOnPricing.TrimStart().StartsWith("{"))
            {
                addOnPricing = _pricingService.SerializeAddOnPricing(cmd.AddOnPricing);
            }

            var customDevelopmentPricing = cmd.CustomDevelopmentPricing;
            if (customDevelopmentPricing != null && !customDevelopmentPricing.TrimStart().StartsWith("{"))
            {
                customDevelopmentPricing = _pricingService.SerializeCustomDevelopmentPricing(cmd.CustomDevelopmentPricing);
            }

            var entity = new Product
            {
                ProductId = Guid.NewGuid(),
                ProductName = cmd.ProductName,
                ProductType = cmd.ProductType,
                Description = cmd.Description,
                CategoryId = cmd.CategoryId,
                BasePricePerUserPerMonth = cmd.BasePricePerUserPerMonth,
                BillingCycleMultipliers = billingCycleMultipliers,
                AddOnPricing = addOnPricing,
                CustomDevelopmentPricing = customDevelopmentPricing,
                Currency = cmd.Currency,
                IsActive = cmd.IsActive,
                CreatedByUserId = cmd.CreatedByUserId,
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.Products.Add(entity);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                var innerException = dbEx.InnerException?.Message ?? dbEx.Message;
                throw new InvalidOperationException($"Database error while saving product: {innerException}", dbEx);
            }

            // Reload entity with navigation properties for mapping
            var entityWithNav = await _db.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == entity.ProductId);

            if (entityWithNav == null)
            {
                throw new InvalidOperationException("Failed to retrieve created product");
            }

            return _mapper.Map<ProductDto>(entityWithNav);
        }
    }
}

