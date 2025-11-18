using FluentValidation;
using CRM.Application.Reports.Commands;

namespace CRM.Application.Reports.Validators
{
    public class SaveDashboardBookmarkCommandValidator : AbstractValidator<SaveDashboardBookmarkCommand>
    {
        public SaveDashboardBookmarkCommandValidator()
        {
            RuleFor(x => x.DashboardConfig)
                .NotNull()
                .WithMessage("DashboardConfig is required");

            RuleFor(x => x.DashboardName)
                .NotEmpty()
                .MaximumLength(200)
                .WithMessage("DashboardName is required and must be less than 200 characters");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId is required");
        }
    }
}

