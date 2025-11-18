using FluentValidation;
using CRM.Application.Notifications.Queries;

namespace CRM.Application.Notifications.Validators
{
    public class GetUnreadCountQueryValidator : AbstractValidator<GetUnreadCountQuery>
    {
        public GetUnreadCountQueryValidator()
        {
            RuleFor(x => x.RequestorUserId)
                .NotEmpty()
                .WithMessage("User ID is required");
        }
    }
}

