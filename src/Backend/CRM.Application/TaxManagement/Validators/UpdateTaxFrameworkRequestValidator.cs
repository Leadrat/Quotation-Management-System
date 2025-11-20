using FluentValidation;
using CRM.Application.TaxManagement.Requests;

namespace CRM.Application.TaxManagement.Validators
{
    public class UpdateTaxFrameworkRequestValidator : AbstractValidator<UpdateTaxFrameworkRequest>
    {
        public UpdateTaxFrameworkRequestValidator()
        {
            RuleFor(x => x.FrameworkName)
                .MinimumLength(2)
                .When(x => !string.IsNullOrWhiteSpace(x.FrameworkName))
                .WithMessage("Framework name must be at least 2 characters")
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.FrameworkName))
                .WithMessage("Framework name cannot exceed 100 characters");

            RuleFor(x => x.TaxComponents)
                .Must(x => x == null || (x != null && x.Count > 0))
                .When(x => x.TaxComponents != null)
                .WithMessage("Tax components cannot be empty if provided");

            When(x => x.TaxComponents != null, () =>
            {
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
            });
        }
    }
}

