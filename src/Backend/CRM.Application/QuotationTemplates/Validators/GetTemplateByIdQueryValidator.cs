using FluentValidation;
using CRM.Application.QuotationTemplates.Queries;

namespace CRM.Application.QuotationTemplates.Validators
{
    public class GetTemplateByIdQueryValidator : AbstractValidator<GetTemplateByIdQuery>
    {
        public GetTemplateByIdQueryValidator()
        {
            RuleFor(x => x.TemplateId)
                .NotEmpty().WithMessage("Template ID is required.");
        }
    }
}

