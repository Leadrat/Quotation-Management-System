using FluentValidation;
using CRM.Application.Users.Commands;
using CRM.Shared.Validation;

namespace CRM.Application.Users.Validators
{
    public class UpdateUserProfileValidator : AbstractValidator<UpdateUserProfileCommand>
    {
        public UpdateUserProfileValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .Matches(ValidationRegex.Name);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .Matches(ValidationRegex.Name);

            When(x => !string.IsNullOrWhiteSpace(x.Mobile), () =>
            {
                RuleFor(x => x.Mobile!)
                    .Matches(ValidationRegex.E164Mobile);
            });
        }
    }
}
