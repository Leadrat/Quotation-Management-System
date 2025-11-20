using FluentValidation;
using CRM.Application.UserManagement.Requests;

namespace CRM.Application.UserManagement.Validators;

public class UpdateUserProfileRequestValidator : AbstractValidator<UpdateUserProfileRequest>
{
    public UpdateUserProfileRequestValidator()
    {
        RuleFor(x => x.AvatarUrl)
            .MaximumLength(500).WithMessage("Avatar URL must not exceed 500 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.AvatarUrl));

        RuleFor(x => x.Bio)
            .MaximumLength(500).WithMessage("Bio must not exceed 500 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Bio));

        RuleFor(x => x.LinkedInUrl)
            .MaximumLength(255).WithMessage("LinkedIn URL must not exceed 255 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.LinkedInUrl));

        RuleFor(x => x.TwitterUrl)
            .MaximumLength(255).WithMessage("Twitter URL must not exceed 255 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.TwitterUrl));
    }
}

