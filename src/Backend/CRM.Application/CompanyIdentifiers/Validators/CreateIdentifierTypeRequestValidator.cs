using CRM.Application.CompanyIdentifiers.DTOs;
using FluentValidation;

namespace CRM.Application.CompanyIdentifiers.Validators
{
    public class CreateIdentifierTypeRequestValidator : AbstractValidator<CreateIdentifierTypeRequest>
    {
        public CreateIdentifierTypeRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(50)
                .Matches(@"^[A-Z_][A-Z0-9_]*$")
                .WithMessage("Name must be uppercase alphanumeric with underscores, starting with a letter or underscore");

            RuleFor(x => x.DisplayName)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(100);

            RuleFor(x => x.Description)
                .MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }
}

