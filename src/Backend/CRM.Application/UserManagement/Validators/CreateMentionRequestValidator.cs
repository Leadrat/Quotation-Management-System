using FluentValidation;
using CRM.Application.UserManagement.Requests;

namespace CRM.Application.UserManagement.Validators;

public class CreateMentionRequestValidator : AbstractValidator<CreateMentionRequest>
{
    public CreateMentionRequestValidator()
    {
        RuleFor(x => x.EntityType)
            .NotEmpty().WithMessage("Entity type is required")
            .Must(et => et == "Comment" || et == "Note")
            .WithMessage("Entity type must be one of: Comment, Note");

        RuleFor(x => x.EntityId)
            .NotEmpty().WithMessage("Entity ID is required");

        RuleFor(x => x.MentionedUserId)
            .NotEmpty().WithMessage("Mentioned user ID is required");
    }
}

