using CRM.Application.Reports.Dtos;

namespace CRM.Application.Reports.Commands
{
    public class ExportReportCommand
    {
        public ExportReportRequest Request { get; set; } = null!;
        public Guid UserId { get; set; }
    }
}

