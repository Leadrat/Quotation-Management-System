using CRM.Application.Reports.Dtos;

namespace CRM.Application.Reports.Commands
{
    public class ScheduleReportCommand
    {
        public ScheduleReportRequest Request { get; set; } = null!;
        public Guid UserId { get; set; }
    }
}

