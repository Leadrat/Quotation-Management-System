using FluentValidation;
using CRM.Application.TaxManagement.Requests;

namespace CRM.Application.TaxManagement.Validators
{
    public class CreateTaxRateRequestValidator : AbstractValidator<CreateTaxRateRequest>
    {
        public CreateTaxRateRequestValidator()
        {
            RuleFor(x => x.TaxFrameworkId)
                .NotEmpty()
                .WithMessage("Tax framework ID is required");

            RuleFor(x => x.TaxRate)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Tax rate must be greater than or equal to 0")
                .LessThanOrEqualTo(100)
                .WithMessage("Tax rate cannot exceed 100");

            RuleFor(x => x.EffectiveFrom)
                .NotEmpty()
                .WithMessage("Effective from date is required");

            RuleFor(x => x.EffectiveTo)
                .GreaterThanOrEqualTo(x => x.EffectiveFrom)
                .WithMessage("Effective to date must be greater than or equal to effective from date")
                .When(x => x.EffectiveTo.HasValue);

            RuleFor(x => x.TaxComponents)
                .NotEmpty()
                .WithMessage("Tax components are required")
                .Must(components => components != null && components.Count > 0)
                .WithMessage("At least one tax component is required");

            RuleForEach(x => x.TaxComponents)
                .ChildRules(component =>
                {
                    component.RuleFor(c => c.Component)
                        .NotEmpty()
                        .WithMessage("Component name is required");

                    component.RuleFor(c => c.Rate)
                        .GreaterThanOrEqualTo(0)
                        .WithMessage("Component rate must be greater than or equal to 0")
                        .LessThanOrEqualTo(100)
                        .WithMessage("Component rate cannot exceed 100");
                });

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));
        }
    }
}

