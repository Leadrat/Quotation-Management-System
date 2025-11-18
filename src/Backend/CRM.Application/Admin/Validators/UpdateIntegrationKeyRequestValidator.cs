using FluentValidation;
using CRM.Application.Admin.Requests;

namespace CRM.Application.Admin.Validators;

public class UpdateIntegrationKeyRequestValidator : AbstractValidator<UpdateIntegrationKeyRequest>
{
    public UpdateIntegrationKeyRequestValidator()
    {
        RuleFor(x => x.KeyName)
            .MaximumLength(255)
            .WithMessage("Key name cannot exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.KeyName));

        RuleFor(x => x.KeyValue)
            .MaximumLength(1000)
            .WithMessage("Key value cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.KeyValue));

        RuleFor(x => x.Provider)
            .MaximumLength(100)
            .WithMessage("Provider cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Provider));
    }
}

