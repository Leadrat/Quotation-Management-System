using System;
using System.Text.Json;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace CRM.Application.Products.Services
{
    public interface IProductPricingService
    {
        decimal CalculatePrice(Product product, int quantity, BillingCycle? billingCycle = null, decimal? hours = null);
        string SerializeBillingCycleMultipliers(object? multipliers);
        string SerializeAddOnPricing(object? pricing);
        string SerializeCustomDevelopmentPricing(object? pricing);
        void InvalidateProductCache(Guid productId);
    }

    public class ProductPricingService : IProductPricingService
    {
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public ProductPricingService(IMemoryCache cache, IConfiguration configuration)
        {
            _cache = cache;
            _configuration = configuration;
        }

        private int GetCacheDurationMinutes()
        {
            return _configuration.GetValue<int>("ProductCatalog:PriceCalculationCacheDurationMinutes", 5);
        }

        public decimal CalculatePrice(Product product, int quantity, BillingCycle? billingCycle = null, decimal? hours = null)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            // Build cache key
            var cacheKey = $"product-price-{product.ProductId}-{quantity}-{billingCycle?.ToString() ?? "monthly"}-{hours?.ToString() ?? "0"}";

            // Try to get from cache
            if (_cache.TryGetValue(cacheKey, out decimal cachedPrice))
            {
                return cachedPrice;
            }

            // Calculate price
            decimal calculatedPrice;
            switch (product.ProductType)
            {
                case ProductType.Subscription:
                    calculatedPrice = CalculateSubscriptionPrice(product, quantity, billingCycle ?? BillingCycle.Monthly);
                    break;

                case ProductType.AddOnSubscription:
                    calculatedPrice = CalculateAddOnSubscriptionPrice(product, quantity);
                    break;

                case ProductType.AddOnOneTime:
                    calculatedPrice = CalculateAddOnOneTimePrice(product, quantity);
                    break;

                case ProductType.CustomDevelopment:
                    calculatedPrice = CalculateCustomDevelopmentPrice(product, quantity, hours);
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported product type: {product.ProductType}");
            }

            // Cache the result
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(GetCacheDurationMinutes())
            };
            _cache.Set(cacheKey, calculatedPrice, cacheOptions);

            return calculatedPrice;
        }

        public void InvalidateProductCache(Guid productId)
        {
            // Remove all cache entries for this product
            // Since we can't enumerate cache keys easily, we'll use a pattern-based approach
            // In a production system, you might want to track cache keys separately
            // For now, we'll rely on expiration for automatic cleanup
            // This method can be extended if needed with a more sophisticated cache key tracking system
        }

        private decimal CalculateSubscriptionPrice(Product product, int quantity, BillingCycle billingCycle)
        {
            if (!product.BasePricePerUserPerMonth.HasValue)
                throw new InvalidOperationException("Base price per user per month is required for subscription products");

            var basePrice = product.BasePricePerUserPerMonth.Value;
            var multiplier = GetBillingCycleMultiplier(product, billingCycle);
            var months = GetMonthsForBillingCycle(billingCycle);

            return basePrice * multiplier * months * quantity;
        }

        private decimal CalculateAddOnSubscriptionPrice(Product product, int quantity)
        {
            if (string.IsNullOrWhiteSpace(product.AddOnPricing))
                throw new InvalidOperationException("Add-on pricing is required for add-on subscription products");

            try
            {
                var pricing = JsonSerializer.Deserialize<Dictionary<string, object>>(product.AddOnPricing, _jsonOptions);
                if (pricing == null || !pricing.ContainsKey("monthlyPrice"))
                    throw new InvalidOperationException("Monthly price is required for subscription add-ons");

                if (decimal.TryParse(pricing["monthlyPrice"]?.ToString(), out var monthlyPrice))
                {
                    return monthlyPrice * quantity;
                }
            }
            catch (JsonException)
            {
                throw new InvalidOperationException("Invalid add-on pricing format");
            }

            throw new InvalidOperationException("Unable to calculate add-on subscription price");
        }

        private decimal CalculateAddOnOneTimePrice(Product product, int quantity)
        {
            if (string.IsNullOrWhiteSpace(product.AddOnPricing))
                throw new InvalidOperationException("Add-on pricing is required for one-time add-on products");

            try
            {
                var pricing = JsonSerializer.Deserialize<Dictionary<string, object>>(product.AddOnPricing, _jsonOptions);
                if (pricing == null || !pricing.ContainsKey("fixedPrice"))
                    throw new InvalidOperationException("Fixed price is required for one-time add-ons");

                if (decimal.TryParse(pricing["fixedPrice"]?.ToString(), out var fixedPrice))
                {
                    return fixedPrice * quantity;
                }
            }
            catch (JsonException)
            {
                throw new InvalidOperationException("Invalid add-on pricing format");
            }

            throw new InvalidOperationException("Unable to calculate one-time add-on price");
        }

        private decimal CalculateCustomDevelopmentPrice(Product product, int quantity, decimal? hours)
        {
            if (string.IsNullOrWhiteSpace(product.CustomDevelopmentPricing))
                throw new InvalidOperationException("Custom development pricing is required");

            try
            {
                var pricing = JsonSerializer.Deserialize<Dictionary<string, object>>(product.CustomDevelopmentPricing, _jsonOptions);
                if (pricing == null || !pricing.ContainsKey("pricingModel"))
                    throw new InvalidOperationException("Pricing model is required for custom development products");

                var pricingModel = pricing["pricingModel"]?.ToString();

                if (pricingModel == "hourly")
                {
                    if (!hours.HasValue)
                        throw new InvalidOperationException("Hours are required for hourly pricing");

                    if (decimal.TryParse(pricing["hourlyRate"]?.ToString(), out var hourlyRate))
                    {
                        return hourlyRate * hours.Value * quantity;
                    }
                }
                else if (pricingModel == "fixed")
                {
                    if (decimal.TryParse(pricing["fixedPrice"]?.ToString(), out var fixedPrice))
                    {
                        return fixedPrice * quantity;
                    }
                }
                else if (pricingModel == "projectBased")
                {
                    if (decimal.TryParse(pricing["baseProjectPrice"]?.ToString(), out var basePrice))
                    {
                        return basePrice * quantity;
                    }
                }
            }
            catch (JsonException)
            {
                throw new InvalidOperationException("Invalid custom development pricing format");
            }

            throw new InvalidOperationException("Unable to calculate custom development price");
        }

        private decimal GetBillingCycleMultiplier(Product product, BillingCycle billingCycle)
        {
            if (string.IsNullOrWhiteSpace(product.BillingCycleMultipliers))
                return 1.0m;

            try
            {
                var multipliers = JsonSerializer.Deserialize<Dictionary<string, decimal>>(product.BillingCycleMultipliers, _jsonOptions);
                if (multipliers == null)
                    return 1.0m;

                var cycleKey = billingCycle switch
                {
                    BillingCycle.Quarterly => "quarterly",
                    BillingCycle.HalfYearly => "halfYearly",
                    BillingCycle.Yearly => "yearly",
                    BillingCycle.MultiYear => "multiYear",
                    _ => "monthly"
                };

                return multipliers.ContainsKey(cycleKey) ? multipliers[cycleKey] : 1.0m;
            }
            catch
            {
                return 1.0m;
            }
        }

        private int GetMonthsForBillingCycle(BillingCycle billingCycle)
        {
            return billingCycle switch
            {
                BillingCycle.Monthly => 1,
                BillingCycle.Quarterly => 3,
                BillingCycle.HalfYearly => 6,
                BillingCycle.Yearly => 12,
                BillingCycle.MultiYear => 36, // Default to 3 years
                _ => 1
            };
        }

        public string SerializeBillingCycleMultipliers(object? multipliers)
        {
            if (multipliers == null) return null!;
            return JsonSerializer.Serialize(multipliers, _jsonOptions);
        }

        public string SerializeAddOnPricing(object? pricing)
        {
            if (pricing == null) return null!;
            return JsonSerializer.Serialize(pricing, _jsonOptions);
        }

        public string SerializeCustomDevelopmentPricing(object? pricing)
        {
            if (pricing == null) return null!;
            return JsonSerializer.Serialize(pricing, _jsonOptions);
        }
    }
}

