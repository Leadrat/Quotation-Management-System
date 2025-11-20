using FluentValidation;
using CRM.Application.Products.Requests;

namespace CRM.Application.Products.Validators
{
    public class UpdateProductCategoryRequestValidator : AbstractValidator<UpdateProductCategoryRequest>
    {
        public UpdateProductCategoryRequestValidator()
        {
            RuleFor(x => x.CategoryName)
                .NotEmpty().WithMessage("Category name is required.")
                .MinimumLength(2).WithMessage("Category name must be at least 2 characters long.")
                .MaximumLength(100).WithMessage("Category name cannot exceed 100 characters.");

            RuleFor(x => x.CategoryCode)
                .NotEmpty().WithMessage("Category code is required.")
                .MaximumLength(50).WithMessage("Category code cannot exceed 50 characters.")
                .Matches("^[A-Z0-9_]+$").WithMessage("Category code must contain only uppercase letters, numbers, and underscores.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");
        }
    }
}

