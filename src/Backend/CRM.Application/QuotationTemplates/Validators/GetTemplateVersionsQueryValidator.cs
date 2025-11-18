using FluentValidation;
using CRM.Application.QuotationTemplates.Queries;

namespace CRM.Application.QuotationTemplates.Validators
{
    public class GetTemplateVersionsQueryValidator : AbstractValidator<GetTemplateVersionsQuery>
    {
        public GetTemplateVersionsQueryValidator()
        {
            RuleFor(x => x.TemplateId)
                .NotEmpty().WithMessage("Template ID is required.");
        }
    }
}

