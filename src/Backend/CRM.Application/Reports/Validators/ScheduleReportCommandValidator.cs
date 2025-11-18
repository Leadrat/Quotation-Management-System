using FluentValidation;
using CRM.Application.Reports.Commands;

namespace CRM.Application.Reports.Validators
{
    public class ScheduleReportCommandValidator : AbstractValidator<ScheduleReportCommand>
    {
        public ScheduleReportCommandValidator()
        {
            RuleFor(x => x.Request)
                .NotNull()
                .WithMessage("Request is required");

            RuleFor(x => x.Request.ReportName)
                .NotEmpty()
                .MaximumLength(200)
                .WithMessage("ReportName is required and must be less than 200 characters");

            RuleFor(x => x.Request.ReportType)
                .NotEmpty()
                .WithMessage("ReportType is required");

            RuleFor(x => x.Request.RecurrencePattern)
                .NotEmpty()
                .Must(p => p.ToLowerInvariant() == "daily" || 
                          p.ToLowerInvariant() == "weekly" || 
                          p.ToLowerInvariant() == "monthly")
                .WithMessage("RecurrencePattern must be daily, weekly, or monthly");

            RuleFor(x => x.Request.EmailRecipients)
                .NotEmpty()
                .WithMessage("EmailRecipients is required");
        }
    }
}

