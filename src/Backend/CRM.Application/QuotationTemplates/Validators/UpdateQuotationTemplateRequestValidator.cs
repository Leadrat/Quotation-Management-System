using FluentValidation;
using CRM.Application.QuotationTemplates.Dtos;
using CRM.Domain.Enums;

namespace CRM.Application.QuotationTemplates.Validators
{
    public class UpdateQuotationTemplateRequestValidator : AbstractValidator<UpdateQuotationTemplateRequest>
    {
        public UpdateQuotationTemplateRequestValidator()
        {
            RuleFor(x => x.Name)
                .Length(3, 100).WithMessage("Template name must be between 3 and 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.Name));

            RuleFor(x => x.Description)
                .MaximumLength(255).WithMessage("Description cannot exceed 255 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.Visibility)
                .Must(v => string.IsNullOrEmpty(v) || Enum.TryParse<TemplateVisibility>(v, true, out _))
                .WithMessage("Visibility must be one of: Public, Team, Private.")
                .When(x => !string.IsNullOrEmpty(x.Visibility));

            RuleFor(x => x.DiscountDefault)
                .InclusiveBetween(0, 100).WithMessage("Discount default must be between 0 and 100.")
                .When(x => x.DiscountDefault.HasValue);

            RuleFor(x => x.Notes)
                .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters.")
                .When(x => !string.IsNullOrEmpty(x.Notes));

            RuleForEach(x => x.LineItems)
                .SetValidator(new UpdateTemplateLineItemRequestValidator())
                .When(x => x.LineItems != null);
        }
    }

    public class UpdateTemplateLineItemRequestValidator : AbstractValidator<UpdateTemplateLineItemRequest>
    {
        public UpdateTemplateLineItemRequestValidator()
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

