using FluentValidation;
using CRM.Application.Reports.Commands;

namespace CRM.Application.Reports.Validators
{
    public class GenerateReportCommandValidator : AbstractValidator<GenerateReportCommand>
    {
        public GenerateReportCommandValidator()
        {
            RuleFor(x => x.Request)
                .NotNull()
                .WithMessage("Request is required");

            RuleFor(x => x.Request.ReportType)
                .NotEmpty()
                .WithMessage("ReportType is required");

            RuleFor(x => x.Request.Format)
                .NotEmpty()
                .WithMessage("Format is required");
        }
    }
}

