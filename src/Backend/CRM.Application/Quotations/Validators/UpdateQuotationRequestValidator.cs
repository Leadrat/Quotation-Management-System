using FluentValidation;
using CRM.Application.Quotations.Dtos;

namespace CRM.Application.Quotations.Validators
{
    public class UpdateQuotationRequestValidator : AbstractValidator<UpdateQuotationRequest>
    {
        public UpdateQuotationRequestValidator()
        {
            RuleFor(x => x.QuotationDate)
                .LessThanOrEqualTo(DateTime.Today)
                .When(x => x.QuotationDate.HasValue)
                .WithMessage("Quotation date cannot be in the future");

            RuleFor(x => x.ValidUntil)
                .GreaterThan(x => x.QuotationDate ?? DateTime.Today)
                .When(x => x.ValidUntil.HasValue && x.QuotationDate.HasValue)
                .WithMessage("Valid until must be after quotation date");

            RuleFor(x => x.DiscountPercentage)
                .InclusiveBetween(0, 100)
                .When(x => x.DiscountPercentage.HasValue)
                .WithMessage("Discount must be between 0 and 100%");

            RuleForEach(x => x.LineItems)
                .SetValidator(new UpdateLineItemRequestValidator())
                .When(x => x.LineItems != null);
        }
    }
}

