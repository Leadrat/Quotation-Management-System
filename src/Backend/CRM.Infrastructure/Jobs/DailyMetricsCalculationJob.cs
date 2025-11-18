using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace CRM.Infrastructure.Jobs
{
    public class DailyMetricsCalculationJob : BackgroundService
    {
        private readonly ILogger<DailyMetricsCalculationJob> _logger;
        private readonly IServiceProvider _serviceProvider;

        public DailyMetricsCalculationJob(
            ILogger<DailyMetricsCalculationJob> logger,
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
                    // Run at 2 AM daily
                    var now = DateTimeOffset.UtcNow;
                    var nextRun = now.Date.AddDays(1).AddHours(2); // Next day at 2 AM
                    var delay = nextRun - now;

                    if (delay.TotalMilliseconds > 0)
                    {
                        await Task.Delay(delay, stoppingToken);
                    }

                    if (stoppingToken.IsCancellationRequested)
                        break;

                    _logger.LogInformation("DailyMetricsCalculationJob started at {Time}", DateTimeOffset.UtcNow);

                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

                    await CalculateDailySalesMetrics(db);
                    await CalculateTeamPerformanceMetrics(db);
                    await CalculatePaymentStatusMetrics(db);
                    await CalculateApprovalMetrics(db);

                    _logger.LogInformation("DailyMetricsCalculationJob completed successfully");
                }
                catch (PostgresException pgEx) when (pgEx.SqlState == "42P01") // Table does not exist
                {
                    _logger.LogWarning("Table does not exist yet, skipping job execution: {Message}", pgEx.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "DailyMetricsCalculationJob failed");
                }

                // Wait 24 hours before next run
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task CalculateDailySalesMetrics(IAppDbContext db)
        {
            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);

            var quotations = await db.Quotations
                .Where(q => q.CreatedAt.Date == yesterday)
                .ToListAsync();

            var metricData = new
            {
                quotationsCreated = quotations.Count,
                quotationsSent = quotations.Count(q => q.Status == Domain.Enums.QuotationStatus.Sent),
                quotationsAccepted = quotations.Count(q => q.Status == Domain.Enums.QuotationStatus.Accepted),
                conversionRate = quotations.Count(q => q.Status == Domain.Enums.QuotationStatus.Sent) > 0
                    ? (decimal)quotations.Count(q => q.Status == Domain.Enums.QuotationStatus.Accepted) /
                      quotations.Count(q => q.Status == Domain.Enums.QuotationStatus.Sent) * 100
                    : 0,
                totalPipelineValue = quotations
                    .Where(q => q.Status == Domain.Enums.QuotationStatus.Draft ||
                               q.Status == Domain.Enums.QuotationStatus.Sent ||
                               q.Status == Domain.Enums.QuotationStatus.Viewed)
                    .Sum(q => q.TotalAmount),
                averageDiscount = quotations.Any() ? quotations.Average(q => q.DiscountPercentage) : 0,
                pendingApprovals = await db.DiscountApprovals
                    .CountAsync(a => a.Status == Domain.Enums.ApprovalStatus.Pending)
            };

            var snapshot = new AnalyticsMetricsSnapshot
            {
                SnapshotId = Guid.NewGuid(),
                MetricType = MetricType.DailySales,
                UserId = null, // Global metric
                MetricData = JsonDocument.Parse(JsonSerializer.Serialize(metricData)),
                CalculatedAt = DateTimeOffset.UtcNow,
                PeriodDate = yesterday,
                CreatedAt = DateTimeOffset.UtcNow
            };

            db.AnalyticsMetricsSnapshots.Add(snapshot);
            await db.SaveChangesAsync();

            _logger.LogInformation("DailySales metrics calculated for {Date}", yesterday);
        }

        private async Task CalculateTeamPerformanceMetrics(IAppDbContext db)
        {
            var today = DateTime.UtcNow.Date;
            var last30Days = today.AddDays(-30);

            var users = await db.Users
                .Where(u => u.IsActive)
                .ToListAsync();

            foreach (var user in users)
            {
                var quotations = await db.Quotations
                    .Where(q => q.CreatedByUserId == user.UserId && q.CreatedAt >= last30Days)
                    .ToListAsync();

                var metricData = new
                {
                    quotationsCreated = quotations.Count,
                    quotationsSent = quotations.Count(q => q.Status == Domain.Enums.QuotationStatus.Sent),
                    quotationsAccepted = quotations.Count(q => q.Status == Domain.Enums.QuotationStatus.Accepted),
                    conversionRate = quotations.Count(q => q.Status == Domain.Enums.QuotationStatus.Sent) > 0
                        ? (decimal)quotations.Count(q => q.Status == Domain.Enums.QuotationStatus.Accepted) /
                          quotations.Count(q => q.Status == Domain.Enums.QuotationStatus.Sent) * 100
                        : 0,
                    totalPipelineValue = quotations
                        .Where(q => q.Status == Domain.Enums.QuotationStatus.Draft ||
                                   q.Status == Domain.Enums.QuotationStatus.Sent ||
                                   q.Status == Domain.Enums.QuotationStatus.Viewed)
                        .Sum(q => q.TotalAmount),
                    averageDiscount = quotations.Any() ? quotations.Average(q => q.DiscountPercentage) : 0
                };

                var snapshot = new AnalyticsMetricsSnapshot
                {
                    SnapshotId = Guid.NewGuid(),
                    MetricType = MetricType.TeamPerformance,
                    UserId = user.UserId,
                    MetricData = JsonDocument.Parse(JsonSerializer.Serialize(metricData)),
                    CalculatedAt = DateTimeOffset.UtcNow,
                    PeriodDate = today,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                db.AnalyticsMetricsSnapshots.Add(snapshot);
            }

            await db.SaveChangesAsync();
            _logger.LogInformation("TeamPerformance metrics calculated for {UserCount} users", users.Count);
        }

        private async Task CalculatePaymentStatusMetrics(IAppDbContext db)
        {
            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);

            var payments = await db.Payments
                .Where(p => p.CreatedAt.Date == yesterday)
                .ToListAsync();

            var metricData = new
            {
                totalPayments = payments.Count,
                successfulPayments = payments.Count(p => p.PaymentStatus == Domain.Enums.PaymentStatus.Success),
                failedPayments = payments.Count(p => p.PaymentStatus == Domain.Enums.PaymentStatus.Failed),
                totalAmount = payments.Sum(p => p.AmountPaid),
                collectionRate = payments.Any()
                    ? (decimal)payments.Count(p => p.PaymentStatus == Domain.Enums.PaymentStatus.Success) /
                      payments.Count * 100
                    : 0
            };

            var snapshot = new AnalyticsMetricsSnapshot
            {
                SnapshotId = Guid.NewGuid(),
                MetricType = MetricType.PaymentStatus,
                UserId = null,
                MetricData = JsonDocument.Parse(JsonSerializer.Serialize(metricData)),
                CalculatedAt = DateTimeOffset.UtcNow,
                PeriodDate = yesterday,
                CreatedAt = DateTimeOffset.UtcNow
            };

            db.AnalyticsMetricsSnapshots.Add(snapshot);
            await db.SaveChangesAsync();

            _logger.LogInformation("PaymentStatus metrics calculated for {Date}", yesterday);
        }

        private async Task CalculateApprovalMetrics(IAppDbContext db)
        {
            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);

            var approvals = await db.DiscountApprovals
                .Where(a => a.RequestDate.Date == yesterday)
                .ToListAsync();

            var metricData = new
            {
                totalRequests = approvals.Count,
                approved = approvals.Count(a => a.Status == Domain.Enums.ApprovalStatus.Approved),
                rejected = approvals.Count(a => a.Status == Domain.Enums.ApprovalStatus.Rejected),
                pending = approvals.Count(a => a.Status == Domain.Enums.ApprovalStatus.Pending),
                averageTAT = approvals
                    .Where(a => a.Status == Domain.Enums.ApprovalStatus.Approved && a.ApprovalDate.HasValue)
                    .Any()
                    ? (decimal)approvals
                        .Where(a => a.Status == Domain.Enums.ApprovalStatus.Approved && a.ApprovalDate.HasValue)
                        .Average(a => (a.ApprovalDate!.Value - a.RequestDate).TotalHours)
                    : 0
            };

            var snapshot = new AnalyticsMetricsSnapshot
            {
                SnapshotId = Guid.NewGuid(),
                MetricType = MetricType.ApprovalMetrics,
                UserId = null,
                MetricData = JsonDocument.Parse(JsonSerializer.Serialize(metricData)),
                CalculatedAt = DateTimeOffset.UtcNow,
                PeriodDate = yesterday,
                CreatedAt = DateTimeOffset.UtcNow
            };

            db.AnalyticsMetricsSnapshots.Add(snapshot);
            await db.SaveChangesAsync();

            _logger.LogInformation("ApprovalMetrics calculated for {Date}", yesterday);
        }
    }
}

