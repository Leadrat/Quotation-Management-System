using System;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Reports.Commands.Handlers
{
    public class CancelScheduledReportCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly ILogger<CancelScheduledReportCommandHandler> _logger;

        public CancelScheduledReportCommandHandler(IAppDbContext db, ILogger<CancelScheduledReportCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task Handle(CancelScheduledReportCommand command)
        {
            var scheduledReport = await _db.ScheduledReports
                .FirstOrDefaultAsync(r => r.ReportId == command.ReportId);

            if (scheduledReport == null)
            {
                throw new InvalidOperationException($"Scheduled report not found: {command.ReportId}");
            }

            // Verify ownership
            if (scheduledReport.CreatedByUserId != command.UserId)
            {
                throw new UnauthorizedAccessException("You do not have permission to cancel this scheduled report");
            }

            scheduledReport.IsActive = false;
            scheduledReport.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();

            _logger.LogInformation("Scheduled report cancelled: {ReportId}", command.ReportId);
        }
    }
}

