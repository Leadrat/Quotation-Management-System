using CRM.Application.Reports.Dtos;

namespace CRM.Application.Reports.Commands
{
    public class GenerateReportCommand
    {
        public ReportGenerationRequest Request { get; set; } = null!;
    }
}

