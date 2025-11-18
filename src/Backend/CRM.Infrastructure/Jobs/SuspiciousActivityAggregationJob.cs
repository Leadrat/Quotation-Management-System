using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.Application.Clients.Services;
using CRM.Application.Common.Persistence;
using CRM.Domain.Entities;
using CRM.Infrastructure.Persistence;
using CRM.Shared.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace CRM.Infrastructure.Jobs
{
    public class SuspiciousActivityAggregationJob : BackgroundService
    {
        private readonly ILogger<SuspiciousActivityAggregationJob> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly SuspiciousActivitySettings _settings;

        public SuspiciousActivityAggregationJob(
            ILogger<SuspiciousActivityAggregationJob> logger,
            IServiceProvider serviceProvider,
            IOptions<SuspiciousActivitySettings> settings)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _settings = settings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var scorer = new SuspiciousActivityScorer(_settings);

                    // Get recent history entries that haven't been flagged yet
                    var recentCutoff = DateTimeOffset.UtcNow.AddMinutes(-10);
                    var unprocessedHistory = await db.ClientHistories
                        .Where(h => h.CreatedAt >= recentCutoff &&
                                   h.SuspicionScore >= _settings.InlineThreshold &&
                                   !db.SuspiciousActivityFlags.Any(f => f.HistoryId == h.HistoryId))
                        .Include(h => h.Client)
                        .ToListAsync(stoppingToken);

                    foreach (var history in unprocessedHistory)
                    {
                        // Get recent history for correlation
                        var recentHistory = await db.ClientHistories
                            .Where(h => h.ClientId == history.ClientId &&
                                       h.CreatedAt >= history.CreatedAt.AddHours(-1) &&
                                       h.CreatedAt <= history.CreatedAt)
                            .ToListAsync(stoppingToken);

                        var score = scorer.CalculateScore(history, recentHistory);
                        var reasons = new List<string>();

                        // Recalculate reasons based on score components
                        if (recentHistory.Count >= _settings.RapidChangeThresholdPerHour)
                        {
                            reasons.Add($"Rapid changes: {recentHistory.Count} changes in last hour");
                        }

                        var hour = history.CreatedAt.Hour;
                        if (hour < 9 || hour >= 18)
                        {
                            reasons.Add($"Unusual time: activity at {hour}:00");
                        }

                        if (history.ChangedFields != null && history.ChangedFields.Count >= 5)
                        {
                            reasons.Add($"Mass update: {history.ChangedFields.Count} fields changed");
                        }

                        if (score >= _settings.InlineThreshold)
                        {
                            var flag = new SuspiciousActivityFlag
                            {
                                FlagId = Guid.NewGuid(),
                                HistoryId = history.HistoryId,
                                ClientId = history.ClientId,
                                Score = score,
                                Reasons = reasons,
                                DetectedAt = DateTimeOffset.UtcNow,
                                Status = "OPEN",
                                Metadata = "{}"
                            };

                            db.SuspiciousActivityFlags.Add(flag);
                        }
                    }

                    var saved = await db.SaveChangesAsync(stoppingToken);
                    if (saved > 0)
                    {
                        _logger.LogInformation("SuspiciousActivityAggregationJob created {Count} flags", saved);
                    }
                }
                catch (PostgresException pgEx) when (pgEx.SqlState == "42P01") // Table does not exist
                {
                    _logger.LogWarning("Table does not exist yet, skipping job execution: {Message}", pgEx.Message);
                    // Wait longer before retrying when tables don't exist
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                    continue;
                }
                catch (NpgsqlException npgEx) when (npgEx.InnerException is System.TimeoutException || npgEx.Message.Contains("Timeout"))
                {
                    _logger.LogWarning("Database connection timeout in SuspiciousActivityAggregationJob, will retry: {Message}", npgEx.Message);
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                    continue;
                }
                catch (System.InvalidOperationException invEx) when (invEx.Message.Contains("transient failure") || invEx.InnerException is System.TimeoutException)
                {
                    _logger.LogWarning("Transient database failure in SuspiciousActivityAggregationJob, will retry: {Message}", invEx.Message);
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                    continue;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "SuspiciousActivityAggregationJob failed");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }

                // Run every 5 minutes
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}

