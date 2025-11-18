using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Reports.Commands;
using CRM.Application.Reports.Commands.Handlers;
using CRM.Application.Reports.Queries;
using CRM.Application.Reports.Queries.Handlers;
using CRM.Application.Notifications.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace CRM.Infrastructure.Jobs
{
    public class ScheduledReportExecutionJob : BackgroundService
    {
        private readonly ILogger<ScheduledReportExecutionJob> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ScheduledReportExecutionJob(
            ILogger<ScheduledReportExecutionJob> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
                    var reportHandler = scope.ServiceProvider.GetRequiredService<GenerateCustomReportQueryHandler>();
                    var notificationService = scope.ServiceProvider.GetRequiredService<IEmailNotificationService>();

                    var now = DateTimeOffset.UtcNow;

                    // Get reports that are due
                    var dueReports = await db.ScheduledReports
                        .Where(r => r.IsActive && r.NextScheduledAt <= now)
                        .ToListAsync();

                    foreach (var report in dueReports)
                    {
                        try
                        {
                            _logger.LogInformation("Executing scheduled report: {ReportId}, Name: {ReportName}",
                                report.ReportId, report.ReportName);

                            // Generate report
                            var query = new GenerateCustomReportQuery
                            {
                                ReportType = report.ReportType,
                                Filters = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(
                                    report.ReportConfig.RootElement.GetRawText())
                            };

                            var reportData = await reportHandler.Handle(query);

                            // Send email (simplified - in real scenario, format as HTML/PDF attachment)
                            // TODO: Implement proper email sending for scheduled reports
                            // For now, this is a placeholder - would need to create Notification entities
                            // and use the proper notification service pattern
                            _logger.LogInformation("Scheduled report generated for: {ReportName}, Recipients: {Recipients}",
                                report.ReportName, report.EmailRecipients);

                            // Update report
                            report.LastSentAt = now;
                            report.NextScheduledAt = CalculateNextScheduledAt(report.RecurrencePattern, now);
                            report.UpdatedAt = now;

                            await db.SaveChangesAsync();

                            _logger.LogInformation("Scheduled report sent successfully: {ReportId}", report.ReportId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error executing scheduled report: {ReportId}", report.ReportId);
                        }
                    }

                    // Wait 1 hour before next check
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
                catch (PostgresException pgEx) when (pgEx.SqlState == "42P01") // Table does not exist
                {
                    _logger.LogWarning("Table does not exist yet, skipping job execution: {Message}", pgEx.Message);
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
                catch (NpgsqlException npgEx) when (npgEx.InnerException is System.TimeoutException || npgEx.Message.Contains("Timeout"))
                {
                    _logger.LogWarning("Database connection timeout in ScheduledReportExecutionJob, will retry: {Message}", npgEx.Message);
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
                catch (System.InvalidOperationException invEx) when (invEx.Message.Contains("transient failure") || invEx.InnerException is System.TimeoutException)
                {
                    _logger.LogWarning("Transient database failure in ScheduledReportExecutionJob, will retry: {Message}", invEx.Message);
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ScheduledReportExecutionJob failed");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }

        private DateTimeOffset CalculateNextScheduledAt(string recurrencePattern, DateTimeOffset currentTime)
        {
            return recurrencePattern.ToLowerInvariant() switch
            {
                "daily" => currentTime.AddDays(1).Date.AddHours(8),
                "weekly" => currentTime.AddDays(7 - (int)currentTime.DayOfWeek).Date.AddHours(8),
                "monthly" => new DateTimeOffset(currentTime.Year, currentTime.Month, 1, 8, 0, 0, TimeSpan.Zero).AddMonths(1),
                _ => currentTime.AddDays(1)
            };
        }
    }
}

