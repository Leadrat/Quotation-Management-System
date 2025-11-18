using CRM.Application.Quotations.Commands;
using FluentValidation;

namespace CRM.Application.Quotations.Validators
{
    public class MarkQuotationAsViewedCommandValidator : AbstractValidator<MarkQuotationAsViewedCommand>
    {
        public MarkQuotationAsViewedCommandValidator()
        {
            RuleFor(x => x.AccessToken)
                .NotEmpty()
                .WithMessage("Access token is required.");

            RuleFor(x => x.IpAddress)
                .MaximumLength(50);

            RuleFor(x => x.UserAgent)
                .MaximumLength(512);
        }
    }
}


