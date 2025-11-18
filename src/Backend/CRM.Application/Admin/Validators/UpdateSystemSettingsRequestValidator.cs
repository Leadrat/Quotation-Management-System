using FluentValidation;
using CRM.Application.Admin.Requests;

namespace CRM.Application.Admin.Validators;

public class UpdateSystemSettingsRequestValidator : AbstractValidator<UpdateSystemSettingsRequest>
{
    public UpdateSystemSettingsRequestValidator()
    {
        RuleFor(x => x.Settings)
            .NotNull()
            .NotEmpty()
            .WithMessage("Settings dictionary cannot be empty");

        RuleForEach(x => x.Settings)
            .Must(kvp => !string.IsNullOrWhiteSpace(kvp.Key))
            .WithMessage("Setting key cannot be empty")
            .Must(kvp => kvp.Key.Length <= 255)
            .WithMessage("Setting key cannot exceed 255 characters");
    }
}

