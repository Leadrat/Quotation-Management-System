using CRM.Application.CompanyIdentifiers.DTOs;
using FluentValidation;

namespace CRM.Application.CompanyIdentifiers.Validators
{
    public class UpdateIdentifierTypeRequestValidator : AbstractValidator<UpdateIdentifierTypeRequest>
    {
        public UpdateIdentifierTypeRequestValidator()
        {
            RuleFor(x => x.DisplayName)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(100);

            RuleFor(x => x.Description)
                .MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }
}

