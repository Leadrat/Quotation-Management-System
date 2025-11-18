using FluentValidation;
using CRM.Application.Notifications.Commands;

namespace CRM.Application.Notifications.Validators
{
    public class UnarchiveNotificationsCommandValidator : AbstractValidator<UnarchiveNotificationsCommand>
    {
        public UnarchiveNotificationsCommandValidator()
        {
            RuleFor(x => x.RequestedByUserId)
                .NotEmpty()
                .WithMessage("User ID is required");

            RuleFor(x => x.NotificationIds)
                .NotEmpty()
                .WithMessage("At least one notification ID must be provided");

            RuleForEach(x => x.NotificationIds)
                .NotEmpty()
                .WithMessage("Notification IDs must be valid GUIDs");
        }
    }
}

