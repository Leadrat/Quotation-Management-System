using FluentValidation;
using CRM.Application.Quotations.Dtos;

namespace CRM.Application.Quotations.Validators
{
    public class CreateQuotationRequestValidator : AbstractValidator<CreateQuotationRequest>
    {
        public CreateQuotationRequestValidator()
        {
            RuleFor(x => x.ClientId)
                .NotEmpty()
                .WithMessage("Client is required");

            RuleFor(x => x.QuotationDate)
                .LessThanOrEqualTo(DateTime.Today)
                .When(x => x.QuotationDate.HasValue)
                .WithMessage("Quotation date cannot be in the future");

            RuleFor(x => x.ValidUntil)
                .GreaterThan(x => x.QuotationDate ?? DateTime.Today)
                .When(x => x.ValidUntil.HasValue)
                .WithMessage("Valid until must be after quotation date");

            RuleFor(x => x.DiscountPercentage)
                .InclusiveBetween(0, 100)
                .WithMessage("Discount must be between 0 and 100%");

            RuleFor(x => x.LineItems)
                .NotEmpty()
                .WithMessage("At least one line item is required");

            RuleForEach(x => x.LineItems)
                .SetValidator(new CreateLineItemRequestValidator());
        }
    }
}

