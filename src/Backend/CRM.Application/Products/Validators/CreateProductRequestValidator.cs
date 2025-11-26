using CRM.Application.Products.Requests;
using CRM.Domain.Enums;
using FluentValidation;
using System.Text.Json;

namespace CRM.Application.Products.Validators
{
    public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
    {
        public CreateProductRequestValidator()
        {
            RuleFor(x => x.ProductName)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.ProductType)
                .IsInEnum()
                .WithMessage("Invalid product type");

            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x.Currency)
                .NotEmpty()
                .Length(3)
                .Matches(@"^[A-Z]{3}$")
                .WithMessage("Currency must be a valid ISO 4217 code (3 uppercase letters)");

            // Subscription product validation
            RuleFor(x => x.BasePricePerUserPerMonth)
                .NotNull()
                .GreaterThan(0)
                .When(x => x.ProductType == ProductType.Subscription)
                .WithMessage("Base price per user per month is required for subscription products");

            RuleFor(x => x.BillingCycleMultipliers)
                .Must(BeValidBillingCycleMultipliers)
                .When(x => !string.IsNullOrWhiteSpace(x.BillingCycleMultipliers))
                .WithMessage("Invalid billing cycle multipliers JSON format");

            // Add-on product validation
            RuleFor(x => x.AddOnPricing)
                .NotNull()
                .Must(BeValidAddOnPricing)
                .When(x => x.ProductType == ProductType.AddOnSubscription || x.ProductType == ProductType.AddOnOneTime)
                .WithMessage("Add-on pricing is required for add-on products");

            // Custom development product validation
            RuleFor(x => x.CustomDevelopmentPricing)
                .NotNull()
                .Must(BeValidCustomDevelopmentPricing)
                .When(x => x.ProductType == ProductType.CustomDevelopment)
                .WithMessage("Custom development pricing is required for custom development products");
        }

        private bool BeValidBillingCycleMultipliers(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return true;

            try
            {
                var multipliers = JsonSerializer.Deserialize<Dictionary<string, decimal>>(json);
                if (multipliers == null)
                    return false;

                foreach (var kvp in multipliers)
                {
                    if (kvp.Value <= 0 || kvp.Value > 1)
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool BeValidAddOnPricing(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return false;

            try
            {
                var pricing = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                if (pricing == null || !pricing.ContainsKey("pricingType"))
                    return false;

                var pricingType = pricing["pricingType"]?.ToString();
                if (pricingType == "subscription")
                {
                    return pricing.ContainsKey("monthlyPrice") && 
                           decimal.TryParse(pricing["monthlyPrice"]?.ToString(), out var monthlyPrice) && 
                           monthlyPrice > 0;
                }
                else if (pricingType == "oneTime")
                {
                    return pricing.ContainsKey("fixedPrice") && 
                           decimal.TryParse(pricing["fixedPrice"]?.ToString(), out var fixedPrice) && 
                           fixedPrice > 0;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private bool BeValidCustomDevelopmentPricing(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return false;

            try
            {
                var pricing = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                if (pricing == null || !pricing.ContainsKey("pricingModel"))
                    return false;

                var pricingModel = pricing["pricingModel"]?.ToString();
                if (pricingModel == "hourly")
                {
                    return pricing.ContainsKey("hourlyRate") && 
                           decimal.TryParse(pricing["hourlyRate"]?.ToString(), out var hourlyRate) && 
                           hourlyRate > 0;
                }
                else if (pricingModel == "fixed")
                {
                    return pricing.ContainsKey("fixedPrice") && 
                           decimal.TryParse(pricing["fixedPrice"]?.ToString(), out var fixedPrice) && 
                           fixedPrice > 0;
                }
                else if (pricingModel == "projectBased")
                {
                    return pricing.ContainsKey("baseProjectPrice") && 
                           decimal.TryParse(pricing["baseProjectPrice"]?.ToString(), out var basePrice) && 
                           basePrice > 0;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}

