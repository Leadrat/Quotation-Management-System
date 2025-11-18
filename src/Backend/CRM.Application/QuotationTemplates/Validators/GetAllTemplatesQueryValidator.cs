using FluentValidation;
using CRM.Application.QuotationTemplates.Queries;

namespace CRM.Application.QuotationTemplates.Validators
{
    public class GetAllTemplatesQueryValidator : AbstractValidator<GetAllTemplatesQuery>
    {
        public GetAllTemplatesQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
        }
    }
}

