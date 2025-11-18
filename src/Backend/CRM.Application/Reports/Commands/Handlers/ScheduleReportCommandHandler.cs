using System;
using System.Text.Json;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Reports.Dtos;
using CRM.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Reports.Commands.Handlers
{
    public class ScheduleReportCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly ILogger<ScheduleReportCommandHandler> _logger;

        public ScheduleReportCommandHandler(IAppDbContext db, ILogger<ScheduleReportCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<ScheduledReport> Handle(ScheduleReportCommand command)
        {
            // Validate recurrence pattern
            if (!IsValidRecurrencePattern(command.Request.RecurrencePattern))
            {
                throw new ArgumentException($"Invalid recurrence pattern: {command.Request.RecurrencePattern}");
            }

            // Calculate NextScheduledAt
            var nextScheduledAt = CalculateNextScheduledAt(command.Request.RecurrencePattern);

            // Create ScheduledReport
            var scheduledReport = new ScheduledReport
            {
                ReportId = Guid.NewGuid(),
                CreatedByUserId = command.UserId,
                ReportName = command.Request.ReportName,
                ReportType = command.Request.ReportType,
                ReportConfig = JsonDocument.Parse(JsonSerializer.Serialize(command.Request.ReportConfig)),
                RecurrencePattern = command.Request.RecurrencePattern.ToLowerInvariant(),
                EmailRecipients = command.Request.EmailRecipients,
                IsActive = true,
                NextScheduledAt = nextScheduledAt,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _db.ScheduledReports.Add(scheduledReport);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Scheduled report created: {ReportId}, Pattern: {Pattern}, Next: {NextScheduledAt}",
                scheduledReport.ReportId, scheduledReport.RecurrencePattern, scheduledReport.NextScheduledAt);

            return scheduledReport;
        }

        private bool IsValidRecurrencePattern(string pattern)
        {
            var validPatterns = new[] { "daily", "weekly", "monthly" };
            return Array.Exists(validPatterns, p => string.Equals(p, pattern, StringComparison.OrdinalIgnoreCase));
        }

        private DateTimeOffset CalculateNextScheduledAt(string recurrencePattern)
        {
            var now = DateTimeOffset.UtcNow;
            
            return recurrencePattern.ToLowerInvariant() switch
            {
                "daily" => now.AddDays(1).Date.AddHours(8), // Next day at 8 AM
                "weekly" => now.AddDays(7 - (int)now.DayOfWeek).Date.AddHours(8), // Next Monday at 8 AM
                "monthly" => new DateTimeOffset(now.Year, now.Month, 1, 8, 0, 0, TimeSpan.Zero).AddMonths(1), // First of next month at 8 AM
                _ => now.AddDays(1)
            };
        }
    }
}

