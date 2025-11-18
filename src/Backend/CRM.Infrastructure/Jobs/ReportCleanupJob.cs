using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Reports.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace CRM.Infrastructure.Jobs
{
    public class ReportCleanupJob : BackgroundService
    {
        private readonly ILogger<ReportCleanupJob> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ReportCleanupJob(
            ILogger<ReportCleanupJob> logger,
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
                    // Run weekly
                    var now = DateTimeOffset.UtcNow;
                    var nextRun = now.AddDays(7);
                    var delay = nextRun - now;

                    if (delay.TotalMilliseconds > 0)
                    {
                        await Task.Delay(delay, stoppingToken);
                    }

                    if (stoppingToken.IsCancellationRequested)
                        break;

                    _logger.LogInformation("ReportCleanupJob started at {Time}", DateTimeOffset.UtcNow);

                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
                    var fileStorage = scope.ServiceProvider.GetRequiredService<IFileStorageService>();

                    // Find exports older than 90 days
                    var cutoffDate = DateTimeOffset.UtcNow.AddDays(-90);
                    var oldExports = await db.ExportedReports
                        .Where(e => e.CreatedAt < cutoffDate)
                        .ToListAsync();

                    foreach (var export in oldExports)
                    {
                        try
                        {
                            // Delete file
                            await fileStorage.DeleteFileAsync(export.FilePath);

                            // Delete record
                            db.ExportedReports.Remove(export);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error deleting export: {ExportId}, File: {FilePath}",
                                export.ExportId, export.FilePath);
                        }
                    }

                    await db.SaveChangesAsync();

                    _logger.LogInformation("ReportCleanupJob deleted {Count} old exports", oldExports.Count);

                    // Wait 7 days before next run
                    await Task.Delay(TimeSpan.FromDays(7), stoppingToken);
                }
                catch (PostgresException pgEx) when (pgEx.SqlState == "42P01") // Table does not exist
                {
                    _logger.LogWarning("Table does not exist yet, skipping job execution: {Message}", pgEx.Message);
                    await Task.Delay(TimeSpan.FromDays(7), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ReportCleanupJob failed");
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
            }
        }
    }
}

