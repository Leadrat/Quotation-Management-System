using FluentValidation;
using CRM.Application.Reports.Commands;

namespace CRM.Application.Reports.Validators
{
    public class ExportReportCommandValidator : AbstractValidator<ExportReportCommand>
    {
        public ExportReportCommandValidator()
        {
            RuleFor(x => x.Request)
                .NotNull()
                .WithMessage("Request is required");

            RuleFor(x => x.Request.ReportId)
                .NotEmpty()
                .WithMessage("ReportId is required");

            RuleFor(x => x.Request.Format)
                .NotEmpty()
                .Must(f => f.ToLowerInvariant() == "pdf" || 
                          f.ToLowerInvariant() == "excel" || 
                          f.ToLowerInvariant() == "csv")
                .WithMessage("Format must be pdf, excel, or csv");
        }
    }
}

