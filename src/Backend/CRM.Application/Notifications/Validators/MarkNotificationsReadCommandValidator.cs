using FluentValidation;
using CRM.Application.Notifications.Commands;

namespace CRM.Application.Notifications.Validators
{
    public class MarkNotificationsReadCommandValidator : AbstractValidator<MarkNotificationsReadCommand>
    {
        public MarkNotificationsReadCommandValidator()
        {
            RuleFor(x => x.RequestedByUserId)
                .NotEmpty()
                .WithMessage("User ID is required");

            RuleFor(x => x.NotificationIds)
                .Must(ids => ids == null || ids.All(id => id != Guid.Empty))
                .WithMessage("Notification IDs must be valid GUIDs")
                .When(x => x.NotificationIds != null);
        }
    }
}

