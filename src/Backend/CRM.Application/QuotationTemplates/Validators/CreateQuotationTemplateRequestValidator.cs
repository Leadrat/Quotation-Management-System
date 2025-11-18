using FluentValidation;
using CRM.Application.QuotationTemplates.Dtos;
using CRM.Domain.Enums;

namespace CRM.Application.QuotationTemplates.Validators
{
    public class CreateQuotationTemplateRequestValidator : AbstractValidator<CreateQuotationTemplateRequest>
    {
        public CreateQuotationTemplateRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Template name is required.")
                .Length(3, 100).WithMessage("Template name must be between 3 and 100 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(255).WithMessage("Description cannot exceed 255 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.Visibility)
                .NotEmpty().WithMessage("Visibility is required.")
                .Must(v => Enum.TryParse<TemplateVisibility>(v, true, out _))
                .WithMessage("Visibility must be one of: Public, Team, Private.");

            RuleFor(x => x.DiscountDefault)
                .InclusiveBetween(0, 100).WithMessage("Discount default must be between 0 and 100.")
                .When(x => x.DiscountDefault.HasValue);

            RuleFor(x => x.Notes)
                .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters.")
                .When(x => !string.IsNullOrEmpty(x.Notes));

            RuleFor(x => x.LineItems)
                .NotEmpty().WithMessage("At least one line item is required.")
                .Must(items => items.Count > 0).WithMessage("At least one line item is required.");

            RuleForEach(x => x.LineItems)
                .SetValidator(new CreateTemplateLineItemRequestValidator());
        }
    }

    public class CreateTemplateLineItemRequestValidator : AbstractValidator<CreateTemplateLineItemRequest>
    {
        public CreateTemplateLineItemRequestValidator()
        {
            RuleFor(x => x.ItemName)
                .NotEmpty().WithMessage("Item name is required.")
                .Length(1, 255).WithMessage("Item name must be between 1 and 255 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

            RuleFor(x => x.UnitRate)
                .GreaterThan(0).WithMessage("Unit rate must be greater than 0.");
        }
    }
}

