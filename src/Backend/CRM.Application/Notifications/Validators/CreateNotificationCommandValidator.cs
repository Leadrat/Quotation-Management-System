using CRM.Application.Notifications.Commands;
using FluentValidation;

namespace CRM.Application.Notifications.Validators;

public class CreateNotificationCommandValidator : AbstractValidator<CreateNotificationCommand>
{
    private static readonly string[] ValidChannels = { "InApp", "Email", "SMS" };

    public CreateNotificationCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(x => x.NotificationTypeId)
            .NotEmpty()
            .WithMessage("NotificationTypeId is required.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(255)
            .WithMessage("Title cannot exceed 255 characters.");

        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Message is required.")
            .MaximumLength(10000)
            .WithMessage("Message cannot exceed 10,000 characters.");

        RuleFor(x => x.SentVia)
            .NotEmpty()
            .WithMessage("SentVia is required.")
            .Must(BeValidChannelCombination)
            .WithMessage("SentVia must contain valid channel combinations (InApp, Email, SMS).");

        RuleFor(x => x.RelatedEntityType)
            .NotEmpty()
            .When(x => x.RelatedEntityId.HasValue)
            .WithMessage("RelatedEntityType is required when RelatedEntityId is provided.")
            .MaximumLength(100)
            .WithMessage("RelatedEntityType cannot exceed 100 characters.");
    }

    private static bool BeValidChannelCombination(string sentVia)
    {
        if (string.IsNullOrWhiteSpace(sentVia))
            return false;

        var channels = sentVia.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.Trim())
            .ToArray();

        return channels.Length > 0 && channels.All(c => ValidChannels.Contains(c));
    }
}
