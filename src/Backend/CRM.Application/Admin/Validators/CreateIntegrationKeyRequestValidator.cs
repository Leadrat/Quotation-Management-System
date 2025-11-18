using FluentValidation;
using CRM.Application.Admin.Requests;

namespace CRM.Application.Admin.Validators;

public class CreateIntegrationKeyRequestValidator : AbstractValidator<CreateIntegrationKeyRequest>
{
    public CreateIntegrationKeyRequestValidator()
    {
        RuleFor(x => x.KeyName)
            .NotEmpty()
            .WithMessage("Key name is required")
            .MaximumLength(255)
            .WithMessage("Key name cannot exceed 255 characters");

        RuleFor(x => x.KeyValue)
            .NotEmpty()
            .WithMessage("Key value is required")
            .MaximumLength(1000)
            .WithMessage("Key value cannot exceed 1000 characters");

        RuleFor(x => x.Provider)
            .NotEmpty()
            .WithMessage("Provider is required")
            .MaximumLength(100)
            .WithMessage("Provider cannot exceed 100 characters");
    }
}

