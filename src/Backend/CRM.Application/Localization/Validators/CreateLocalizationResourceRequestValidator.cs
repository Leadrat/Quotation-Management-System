using FluentValidation;
using CRM.Application.Localization.Dtos;

namespace CRM.Application.Localization.Validators;

public class CreateLocalizationResourceRequestValidator : AbstractValidator<CreateLocalizationResourceRequest>
{
    public CreateLocalizationResourceRequestValidator()
    {
        RuleFor(x => x.LanguageCode)
            .NotEmpty()
            .MaximumLength(5);

        RuleFor(x => x.ResourceKey)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.ResourceValue)
            .NotEmpty()
            .MaximumLength(1000);

        RuleFor(x => x.Category)
            .MaximumLength(50)
            .When(x => x.Category != null);
    }
}

