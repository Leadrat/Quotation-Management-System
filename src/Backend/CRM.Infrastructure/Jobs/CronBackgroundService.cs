using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Quartz;

namespace CRM.Infrastructure.Jobs
{
    public abstract class CronBackgroundService : BackgroundService
    {
        private readonly CronExpression _cronExpression;
        protected readonly ILogger? Logger;

        protected CronBackgroundService(string cronExpression, TimeZoneInfo? timeZone = null, ILogger? logger = null)
        {
            _cronExpression = new CronExpression(cronExpression)
            {
                TimeZone = timeZone ?? TimeZoneInfo.Local
            };
            Logger = logger;
        }

        protected abstract Task ProcessAsync(CancellationToken stoppingToken);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var next = _cronExpression.GetNextValidTimeAfter(DateTimeOffset.UtcNow);
                if (!next.HasValue)
                {
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                    continue;
                }

                var delay = next.Value - DateTimeOffset.UtcNow;
                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, stoppingToken);
                }

                try
                {
                    await ProcessAsync(stoppingToken);
                }
                catch (PostgresException pgEx) when (pgEx.SqlState == "42P01") // Table does not exist
                {
                    Logger?.LogWarning("Table does not exist yet, skipping job execution: {Message}", pgEx.Message);
                    // Don't throw - allow the job to continue running
                }
                catch (NpgsqlException npgEx) when (npgEx.InnerException is System.TimeoutException || npgEx.Message.Contains("Timeout"))
                {
                    Logger?.LogWarning("Database connection timeout in {JobType}, will retry on next schedule: {Message}", GetType().Name, npgEx.Message);
                    // Don't throw - allow the job to continue running, will retry on next schedule
                }
                catch (System.InvalidOperationException invEx) when (invEx.Message.Contains("transient failure") || invEx.InnerException is System.TimeoutException)
                {
                    Logger?.LogWarning("Transient database failure in {JobType}, will retry on next schedule: {Message}", GetType().Name, invEx.Message);
                    // Don't throw - allow the job to continue running, will retry on next schedule
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex, "Error in background job {JobType}", GetType().Name);
                    if (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    // For other exceptions, log but don't crash the host
                    // The job will retry on the next schedule
                }
            }
        }
    }
}


