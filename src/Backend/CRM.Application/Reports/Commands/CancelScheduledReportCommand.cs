using System;

namespace CRM.Application.Reports.Commands
{
    public class CancelScheduledReportCommand
    {
        public Guid ReportId { get; set; }
        public Guid UserId { get; set; }
    }
}

