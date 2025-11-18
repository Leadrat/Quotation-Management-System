using FluentValidation;
using CRM.Application.Notifications.Queries;

namespace CRM.Application.Notifications.Validators
{
    public class GetNotificationPreferencesQueryValidator : AbstractValidator<GetNotificationPreferencesQuery>
    {
        public GetNotificationPreferencesQueryValidator()
        {
            RuleFor(x => x.RequestorUserId)
                .NotEmpty()
                .WithMessage("User ID is required");
        }
    }
}

