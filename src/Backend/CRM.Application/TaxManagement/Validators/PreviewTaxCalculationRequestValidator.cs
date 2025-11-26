using FluentValidation;
using CRM.Application.TaxManagement.Requests;

namespace CRM.Application.TaxManagement.Validators
{
    public class PreviewTaxCalculationRequestValidator : AbstractValidator<PreviewTaxCalculationRequest>
    {
        public PreviewTaxCalculationRequestValidator()
        {
            RuleFor(x => x.ClientId)
                .NotEmpty()
                .WithMessage("Client ID is required");

            RuleFor(x => x.LineItems)
                .NotEmpty()
                .WithMessage("At least one line item is required");

            RuleForEach(x => x.LineItems)
                .ChildRules(lineItem =>
                {
                    lineItem.RuleFor(li => li.Amount)
                        .GreaterThan(0)
                        .WithMessage("Line item amount must be greater than 0");
                });

            RuleFor(x => x.Subtotal)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Subtotal must be greater than or equal to 0");

            RuleFor(x => x.DiscountAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Discount amount must be greater than or equal to 0")
                .LessThanOrEqualTo(x => x.Subtotal)
                .WithMessage("Discount amount cannot exceed subtotal");
        }
    }
}

