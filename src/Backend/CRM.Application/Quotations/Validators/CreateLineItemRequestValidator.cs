using FluentValidation;
using CRM.Application.Quotations.Dtos;

namespace CRM.Application.Quotations.Validators
{
    public class CreateLineItemRequestValidator : AbstractValidator<CreateLineItemRequest>
    {
        public CreateLineItemRequestValidator()
        {
            RuleFor(x => x.ItemName)
                .NotEmpty()
                .WithMessage("Item name is required")
                .MinimumLength(2)
                .WithMessage("Item name must be at least 2 characters")
                .MaximumLength(255)
                .WithMessage("Item name cannot exceed 255 characters");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than 0")
                .LessThanOrEqualTo(9999999.99m)
                .WithMessage("Quantity is too large");

            RuleFor(x => x.UnitRate)
                .GreaterThan(0)
                .WithMessage("Unit rate must be greater than 0")
                .LessThanOrEqualTo(999999.99m)
                .WithMessage("Unit rate is too large");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .When(x => !string.IsNullOrWhiteSpace(x.Description))
                .WithMessage("Description cannot exceed 1000 characters");
        }
    }
}

