using FluentValidation;
using CRM.Application.Admin.Requests;

namespace CRM.Application.Admin.Validators;

public class UpdateDataRetentionPolicyRequestValidator : AbstractValidator<UpdateDataRetentionPolicyRequest>
{
    public UpdateDataRetentionPolicyRequestValidator()
    {
        RuleFor(x => x.EntityType)
            .NotEmpty()
            .WithMessage("Entity type is required")
            .MaximumLength(100)
            .WithMessage("Entity type cannot exceed 100 characters");

        RuleFor(x => x.RetentionPeriodMonths)
            .GreaterThan(0)
            .WithMessage("Retention period must be greater than 0")
            .LessThanOrEqualTo(120)
            .WithMessage("Retention period cannot exceed 120 months (10 years)");
    }
}

