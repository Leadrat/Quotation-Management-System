using FluentValidation;
using CRM.Application.Auth.Commands;
using CRM.Shared.Security;

namespace CRM.Application.Auth.Validators
{
    public class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.CurrentPassword).NotEmpty();
            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .Matches(PasswordPolicy.StrengthPattern);
            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.NewPassword).WithMessage("Passwords do not match");
        }
    }
}
