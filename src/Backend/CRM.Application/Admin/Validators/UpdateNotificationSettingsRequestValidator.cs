using FluentValidation;
using CRM.Application.Admin.Requests;

namespace CRM.Application.Admin.Validators;

public class UpdateNotificationSettingsRequestValidator : AbstractValidator<UpdateNotificationSettingsRequest>
{
    public UpdateNotificationSettingsRequestValidator()
    {
        RuleFor(x => x.BannerType)
            .Must(type => type == null || new[] { "info", "warning", "error" }.Contains(type))
            .WithMessage("Banner type must be one of: info, warning, error")
            .When(x => !string.IsNullOrEmpty(x.BannerType));

        RuleFor(x => x.BannerMessage)
            .MaximumLength(1000)
            .WithMessage("Banner message cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.BannerMessage));

        RuleFor(x => x.BannerMessage)
            .NotEmpty()
            .WithMessage("Banner message is required when banner is visible")
            .When(x => x.IsVisible);
    }
}

