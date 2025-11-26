using FluentValidation;
using CRM.Application.TaxManagement.Requests;

namespace CRM.Application.TaxManagement.Validators
{
    public class CreateProductServiceCategoryRequestValidator : AbstractValidator<CreateProductServiceCategoryRequest>
    {
        public CreateProductServiceCategoryRequestValidator()
        {
            RuleFor(x => x.CategoryName)
                .NotEmpty()
                .WithMessage("Category name is required")
                .MinimumLength(2)
                .WithMessage("Category name must be at least 2 characters")
                .MaximumLength(100)
                .WithMessage("Category name cannot exceed 100 characters");

            RuleFor(x => x.CategoryCode)
                .MaximumLength(20)
                .WithMessage("Category code cannot exceed 20 characters")
                .When(x => !string.IsNullOrEmpty(x.CategoryCode));

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));
        }
    }
}

