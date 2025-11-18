using FluentValidation;
using CRM.Application.Notifications.Queries;

namespace CRM.Application.Notifications.Validators
{
    public class GetEntityNotificationsQueryValidator : AbstractValidator<GetEntityNotificationsQuery>
    {
        public GetEntityNotificationsQueryValidator()
        {
            RuleFor(x => x.RequestorUserId)
                .NotEmpty()
                .WithMessage("User ID is required");

            RuleFor(x => x.EntityType)
                .NotEmpty()
                .WithMessage("Entity type is required")
                .MaximumLength(50)
                .WithMessage("Entity type cannot exceed 50 characters");

            RuleFor(x => x.EntityId)
                .NotEmpty()
                .WithMessage("Entity ID is required");
        }
    }
}

