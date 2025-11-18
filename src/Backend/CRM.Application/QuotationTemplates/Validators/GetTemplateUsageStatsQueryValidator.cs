using FluentValidation;
using CRM.Application.QuotationTemplates.Queries;

namespace CRM.Application.QuotationTemplates.Validators
{
    public class GetTemplateUsageStatsQueryValidator : AbstractValidator<GetTemplateUsageStatsQuery>
    {
        public GetTemplateUsageStatsQueryValidator()
        {
            RuleFor(x => x.RequestorUserId)
                .NotEmpty().WithMessage("Requestor user ID is required.");

            RuleFor(x => x)
                .Must(x => !x.StartDate.HasValue || !x.EndDate.HasValue || x.StartDate <= x.EndDate)
                .WithMessage("Start date must be less than or equal to end date.")
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
        }
    }
}

