using FluentValidation;
using CRM.Application.TaxManagement.Requests;

namespace CRM.Application.TaxManagement.Validators
{
    public class CreateTaxFrameworkRequestValidator : AbstractValidator<CreateTaxFrameworkRequest>
    {
        public CreateTaxFrameworkRequestValidator()
        {
            RuleFor(x => x.CountryId)
                .NotEmpty()
                .WithMessage("Country ID is required");

            RuleFor(x => x.FrameworkName)
                .NotEmpty()
                .WithMessage("Framework name is required")
                .MinimumLength(2)
                .WithMessage("Framework name must be at least 2 characters")
                .MaximumLength(100)
                .WithMessage("Framework name cannot exceed 100 characters");

            RuleFor(x => x.TaxComponents)
                .NotEmpty()
                .WithMessage("Tax components are required")
                .Must(x => x != null && x.Count > 0)
                .WithMessage("At least one tax component is required");

            RuleForEach(x => x.TaxComponents)
                .ChildRules(component =>
                {
                    component.RuleFor(c => c.Name)
                        .NotEmpty()
                        .WithMessage("Component name is required");

                    component.RuleFor(c => c.Code)
                        .NotEmpty()
                        .WithMessage("Component code is required");
                });
        }
    }
}

