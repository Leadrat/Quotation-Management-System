using System.Text.RegularExpressions;
using FluentValidation;
using CRM.Application.Admin.Requests;

namespace CRM.Application.Admin.Validators;

public class UpdateBrandingRequestValidator : AbstractValidator<UpdateBrandingRequest>
{
    private static readonly Regex HexColorRegex = new(@"^#[0-9A-Fa-f]{6}$", RegexOptions.Compiled);

    public UpdateBrandingRequestValidator()
    {
        RuleFor(x => x.PrimaryColor)
            .Matches(HexColorRegex)
            .WithMessage("Primary color must be a valid hex color code (e.g., #FF5733)")
            .When(x => !string.IsNullOrEmpty(x.PrimaryColor));

        RuleFor(x => x.SecondaryColor)
            .Matches(HexColorRegex)
            .WithMessage("Secondary color must be a valid hex color code (e.g., #FF5733)")
            .When(x => !string.IsNullOrEmpty(x.SecondaryColor));

        RuleFor(x => x.AccentColor)
            .Matches(HexColorRegex)
            .WithMessage("Accent color must be a valid hex color code (e.g., #FF5733)")
            .When(x => !string.IsNullOrEmpty(x.AccentColor));

        RuleFor(x => x.FooterHtml)
            .MaximumLength(5000)
            .WithMessage("Footer HTML cannot exceed 5000 characters")
            .When(x => !string.IsNullOrEmpty(x.FooterHtml));
    }
}

