using FluentValidation;
using CRM.Application.TaxManagement.Requests;

namespace CRM.Application.TaxManagement.Validators
{
    public class UpdateJurisdictionRequestValidator : AbstractValidator<UpdateJurisdictionRequest>
    {
        public UpdateJurisdictionRequestValidator()
        {
            RuleFor(x => x.JurisdictionName)
                .NotEmpty()
                .WithMessage("Jurisdiction name is required")
                .MinimumLength(2)
                .WithMessage("Jurisdiction name must be at least 2 characters")
                .MaximumLength(100)
                .WithMessage("Jurisdiction name cannot exceed 100 characters");

            RuleFor(x => x.JurisdictionCode)
                .MaximumLength(20)
                .WithMessage("Jurisdiction code cannot exceed 20 characters")
                .When(x => !string.IsNullOrEmpty(x.JurisdictionCode));

            RuleFor(x => x.JurisdictionType)
                .MaximumLength(20)
                .WithMessage("Jurisdiction type cannot exceed 20 characters")
                .When(x => !string.IsNullOrEmpty(x.JurisdictionType));
        }
    }
}

