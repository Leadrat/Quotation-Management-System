using FluentValidation;
using CRM.Application.Notifications.Commands;

namespace CRM.Application.Notifications.Validators
{
    public class UpdateNotificationPreferencesCommandValidator : AbstractValidator<UpdateNotificationPreferencesCommand>
    {
        public UpdateNotificationPreferencesCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required");

            RuleFor(x => x.Preferences)
                .NotNull()
                .WithMessage("Preferences are required");

            // Validate preference structure: each event type should have valid channel settings
            RuleForEach(x => x.Preferences)
                .Must(kvp => kvp.Value != null)
                .WithMessage("Preference settings for each event type must not be null");

            // Validate that each preference has valid channel keys (inApp, email, push, muted)
            RuleForEach(x => x.Preferences)
                .Must(kvp => kvp.Value.Keys.All(key => 
                    key == "inApp" || key == "email" || key == "push" || key == "muted"))
                .WithMessage("Invalid channel key. Must be one of: inApp, email, push, muted")
                .When(x => x.Preferences != null);
        }
    }
}

