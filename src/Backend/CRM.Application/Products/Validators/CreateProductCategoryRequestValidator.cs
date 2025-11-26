using CRM.Application.Products.Requests;
using FluentValidation;
using System.Text.RegularExpressions;

namespace CRM.Application.Products.Validators
{
    public class CreateProductCategoryRequestValidator : AbstractValidator<CreateProductCategoryRequest>
    {
        public CreateProductCategoryRequestValidator()
        {
            RuleFor(x => x.CategoryName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.CategoryCode)
                .NotEmpty()
                .MaximumLength(50)
                .Matches(@"^[A-Z0-9_]+$")
                .WithMessage("Category code must contain only uppercase letters, numbers, and underscores");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }
}

